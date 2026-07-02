using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Data;

public class DataVersionManager : IDataVersionManager
{
    private readonly string _connectionString;
    private readonly ILogger<DataVersionManager>? _logger;

    public DataVersionManager(string connectionString, ILogger<DataVersionManager>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<bool> IsVersionValidAsync(string setVersion, string dataVersion, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT COUNT(*) 
            FROM BigDataMeta 
            WHERE SetVersion = @SetVersion AND Version = @Version;
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@SetVersion", setVersion);
        command.Parameters.AddWithValue("@Version", dataVersion);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
        return count > 0;
    }

    public async Task<string?> GetLatestVersionAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Version 
            FROM BigDataMeta 
            WHERE SetVersion = @SetVersion 
            ORDER BY LastUpdated DESC 
            LIMIT 1;
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@SetVersion", setVersion);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result as string;
    }

    public async Task<MetaData> CreateOrUpdateVersionAsync(MetaData metaData, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT OR REPLACE INTO BigDataMeta (
                Id, Version, SetVersion, LastUpdated, DataSource, TotalMatches, TotalPlayers, CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @Version, @SetVersion, @LastUpdated, @DataSource, @TotalMatches, @TotalPlayers, @CreatedAt, @UpdatedAt
            );
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", metaData.Id);
        command.Parameters.AddWithValue("@Version", metaData.Version);
        command.Parameters.AddWithValue("@SetVersion", metaData.SetVersion);
        command.Parameters.AddWithValue("@LastUpdated", metaData.LastUpdated.ToString("O"));
        command.Parameters.AddWithValue("@DataSource", metaData.DataSource);
        command.Parameters.AddWithValue("@TotalMatches", metaData.TotalMatches);
        command.Parameters.AddWithValue("@TotalPlayers", metaData.TotalPlayers);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogInformation("Created or updated version: {Version} for set {SetVersion}", metaData.Version, metaData.SetVersion);

        return metaData;
    }

    public async Task<IEnumerable<MetaData>> GetVersionHistoryAsync(string setVersion, int limit = 10, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Version, SetVersion, LastUpdated, DataSource, TotalMatches, TotalPlayers 
            FROM BigDataMeta 
            WHERE SetVersion = @SetVersion 
            ORDER BY LastUpdated DESC 
            LIMIT @Limit;
        ";

        var versions = new List<MetaData>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@SetVersion", setVersion);
        command.Parameters.AddWithValue("@Limit", limit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            versions.Add(new MetaData
            {
                Id = reader.GetString(0),
                Version = reader.GetString(1),
                SetVersion = reader.GetString(2),
                LastUpdated = DateTime.Parse(reader.GetString(3)),
                DataSource = reader.GetString(4),
                TotalMatches = reader.GetInt32(5),
                TotalPlayers = reader.GetInt32(6)
            });
        }

        return versions;
    }

    public async Task<bool> CheckDataConsistencyAsync(string setVersion, string version, CancellationToken cancellationToken = default)
    {
        var result = await PerformConsistencyCheckAsync(setVersion, cancellationToken);
        return result.IsConsistent && result.Version == version;
    }

    public async Task<ConsistencyCheckResult> PerformConsistencyCheckAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        var result = new ConsistencyCheckResult
        {
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // 获取最新版本
            var latestVersion = await GetLatestVersionAsync(setVersion, cancellationToken);
            if (latestVersion == null)
            {
                result.Issues.Add("No data found for this set version");
                return result;
            }

            result.Version = latestVersion;

            // 检查英雄数据
            var championCount = await GetRecordCountAsync(connection, "BigDataChampions", setVersion, cancellationToken);
            result.TotalChampions = championCount;
            if (championCount == 0)
            {
                result.Issues.Add("No champion data found");
            }

            // 检查阵容数据
            var lineupCount = await GetRecordCountAsync(connection, "BigDataLineups", setVersion, cancellationToken);
            result.TotalLineups = lineupCount;
            if (lineupCount == 0)
            {
                result.Issues.Add("No lineup data found");
            }

            // 检查装备数据
            var equipmentCount = await GetRecordCountAsync(connection, "BigDataEquipment", setVersion, cancellationToken);
            result.TotalEquipment = equipmentCount;
            if (equipmentCount == 0)
            {
                result.Issues.Add("No equipment data found");
            }

            result.IsConsistent = result.Issues.Count == 0;
            _logger?.LogInformation("Consistency check completed for set {SetVersion}, version {Version}: {IsConsistent}", 
                setVersion, result.Version, result.IsConsistent);

        }
        catch (Exception ex)
        {
            result.Issues.Add($"Error during consistency check: {ex.Message}");
            _logger?.LogError(ex, "Error performing consistency check");
        }

        return result;
    }

    public async Task DeleteOldVersionsAsync(string setVersion, int keepLatest = 3, CancellationToken cancellationToken = default)
    {
        // 获取要保留的版本
        var versionsToKeep = await GetVersionHistoryAsync(setVersion, keepLatest, cancellationToken);
        var versionIdsToKeep = versionsToKeep.Select(v => v.Id).ToHashSet();

        // 删除旧版本的元数据
        const string metaSql = @"
            DELETE FROM BigDataMeta 
            WHERE SetVersion = @SetVersion AND Id NOT IN ({0});
        ";

        // 构建参数占位符
        var placeholders = string.Join(", ", versionIdsToKeep.Select((_, i) => $"@Id{i}"));
        var metaQuery = string.Format(metaSql, placeholders);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var metaCommand = new SqliteCommand(metaQuery, connection);
        metaCommand.Parameters.AddWithValue("@SetVersion", setVersion);

        for (int i = 0; i < versionIdsToKeep.Count; i++)
        {
            metaCommand.Parameters.AddWithValue($"@Id{i}", versionIdsToKeep.ElementAt(i));
        }

        var deletedCount = await metaCommand.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogInformation("Deleted {Count} old versions for set {SetVersion}", deletedCount, setVersion);
    }

    private async Task<int> GetRecordCountAsync(SqliteConnection connection, string tableName, string setVersion, CancellationToken cancellationToken)
    {
        const string sql = "SELECT COUNT(*) FROM {0} WHERE SetVersion = @SetVersion;";
        var query = string.Format(sql, tableName);

        await using var command = new SqliteCommand(query, connection);
        command.Parameters.AddWithValue("@SetVersion", setVersion);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }
}
