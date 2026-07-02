using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Data;

public class BigDataRepository
{
    private readonly string _connectionString;
    private readonly ILogger<BigDataRepository>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public BigDataRepository(string connectionString, ILogger<BigDataRepository>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    // MetaData operations
    public async Task SaveMetaDataAsync(MetaData metaData, CancellationToken cancellationToken = default)
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
        _logger?.LogDebug("Saved MetaData with Id: {Id}", metaData.Id);
    }

    public async Task<MetaData?> GetMetaDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Version, SetVersion, LastUpdated, DataSource, TotalMatches, TotalPlayers
            FROM BigDataMeta
            WHERE SetVersion = @SetVersion
            ORDER BY LastUpdated DESC
            LIMIT 1;
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@SetVersion", setVersion);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new MetaData
            {
                Id = reader.GetString(0),
                Version = reader.GetString(1),
                SetVersion = reader.GetString(2),
                LastUpdated = DateTime.Parse(reader.GetString(3)),
                DataSource = reader.GetString(4),
                TotalMatches = reader.GetInt32(5),
                TotalPlayers = reader.GetInt32(6)
            };
        }

        return null;
    }

    // LineupData operations
    public async Task SaveLineupDataAsync(LineupData lineupData, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT OR REPLACE INTO BigDataLineups (
                Id, Name, SetVersion, ChampionsJson, TraitsJson, ChampionItemsJson, WinRate, PickRate, MatchCount, AveragePlacement, LastUpdated, CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @Name, @SetVersion, @ChampionsJson, @TraitsJson, @ChampionItemsJson, @WinRate, @PickRate, @MatchCount, @AveragePlacement, @LastUpdated, @CreatedAt, @UpdatedAt
            );
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", lineupData.Id);
        command.Parameters.AddWithValue("@Name", lineupData.Name);
        command.Parameters.AddWithValue("@SetVersion", lineupData.SetVersion);
        command.Parameters.AddWithValue("@ChampionsJson", JsonSerializer.Serialize(lineupData.Champions, _jsonOptions));
        command.Parameters.AddWithValue("@TraitsJson", JsonSerializer.Serialize(lineupData.Traits, _jsonOptions));
        command.Parameters.AddWithValue("@ChampionItemsJson", JsonSerializer.Serialize(lineupData.ChampionItems, _jsonOptions));
        command.Parameters.AddWithValue("@WinRate", lineupData.WinRate);
        command.Parameters.AddWithValue("@PickRate", lineupData.PickRate);
        command.Parameters.AddWithValue("@MatchCount", lineupData.MatchCount);
        command.Parameters.AddWithValue("@AveragePlacement", lineupData.AveragePlacement);
        command.Parameters.AddWithValue("@LastUpdated", lineupData.LastUpdated.ToString("O"));
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogDebug("Saved LineupData with Id: {Id}", lineupData.Id);
    }

    public async Task<IEnumerable<LineupData>> GetLineupDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, SetVersion, ChampionsJson, TraitsJson, ChampionItemsJson, WinRate, PickRate, MatchCount, AveragePlacement, LastUpdated
            FROM BigDataLineups
            WHERE SetVersion = @SetVersion
            ORDER BY WinRate DESC;
        ";

        var lineups = new List<LineupData>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@SetVersion", setVersion);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var lineup = new LineupData
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                SetVersion = reader.GetString(2),
                Champions = JsonSerializer.Deserialize<List<string>>(reader.GetString(3), _jsonOptions) ?? new List<string>(),
                Traits = JsonSerializer.Deserialize<List<string>>(reader.GetString(4), _jsonOptions) ?? new List<string>(),
                ChampionItems = JsonSerializer.Deserialize<List<ChampionItem>>(reader.GetString(5), _jsonOptions) ?? new List<ChampionItem>(),
                WinRate = reader.GetDouble(6),
                PickRate = reader.GetDouble(7),
                MatchCount = reader.GetInt32(8),
                AveragePlacement = reader.GetInt32(9),
                LastUpdated = DateTime.Parse(reader.GetString(10))
            };
            lineups.Add(lineup);
        }

        return lineups;
    }

    // EquipmentData operations
    public async Task SaveEquipmentDataAsync(EquipmentData equipmentData, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT OR REPLACE INTO BigDataEquipment (
                Id, Name, SetVersion, Description, ImageUrl, IsComponent, ComponentsJson, WinRate, Top4Rate, Top1Rate, PickRate, EconomicValue, EquipmentType, MatchCount, LastUpdated, CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @Name, @SetVersion, @Description, @ImageUrl, @IsComponent, @ComponentsJson, @WinRate, @Top4Rate, @Top1Rate, @PickRate, @EconomicValue, @EquipmentType, @MatchCount, @LastUpdated, @CreatedAt, @UpdatedAt
            );
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", equipmentData.Id);
        command.Parameters.AddWithValue("@Name", equipmentData.Name);
        command.Parameters.AddWithValue("@SetVersion", equipmentData.SetVersion);
        command.Parameters.AddWithValue("@Description", equipmentData.Description);
        command.Parameters.AddWithValue("@ImageUrl", equipmentData.ImageUrl);
        command.Parameters.AddWithValue("@IsComponent", equipmentData.IsComponent ? 1 : 0);
        command.Parameters.AddWithValue("@ComponentsJson", JsonSerializer.Serialize(equipmentData.Components, _jsonOptions));
        command.Parameters.AddWithValue("@WinRate", equipmentData.WinRate);
        command.Parameters.AddWithValue("@Top4Rate", equipmentData.Top4Rate);
        command.Parameters.AddWithValue("@Top1Rate", equipmentData.Top1Rate);
        command.Parameters.AddWithValue("@PickRate", equipmentData.PickRate);
        command.Parameters.AddWithValue("@EconomicValue", equipmentData.EconomicValue);
        command.Parameters.AddWithValue("@EquipmentType", equipmentData.EquipmentType);
        command.Parameters.AddWithValue("@MatchCount", equipmentData.MatchCount);
        command.Parameters.AddWithValue("@LastUpdated", equipmentData.LastUpdated.ToString("O"));
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogDebug("Saved EquipmentData with Id: {Id}", equipmentData.Id);
    }

    public async Task<IEnumerable<EquipmentData>> GetEquipmentDataAsync(string setVersion, bool? isComponent = null, CancellationToken cancellationToken = default)
    {
        string sql = @"
            SELECT Id, Name, SetVersion, Description, ImageUrl, IsComponent, ComponentsJson, WinRate, Top4Rate, Top1Rate, PickRate, EconomicValue, EquipmentType, MatchCount, LastUpdated
            FROM BigDataEquipment
            WHERE SetVersion = @SetVersion
        ";

        if (isComponent.HasValue)
        {
            sql += " AND IsComponent = @IsComponent";
        }

        sql += " ORDER BY WinRate DESC;";

        var equipmentList = new List<EquipmentData>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@SetVersion", setVersion);

        if (isComponent.HasValue)
        {
            command.Parameters.AddWithValue("@IsComponent", isComponent.Value ? 1 : 0);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var equipment = new EquipmentData
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                SetVersion = reader.GetString(2),
                Description = reader.GetString(3),
                ImageUrl = reader.GetString(4),
                IsComponent = reader.GetInt32(5) == 1,
                Components = JsonSerializer.Deserialize<List<string>>(reader.GetString(6), _jsonOptions) ?? new List<string>(),
                WinRate = reader.GetDouble(7),
                Top4Rate = reader.GetDouble(8),
                Top1Rate = reader.GetDouble(9),
                PickRate = reader.GetDouble(10),
                EconomicValue = reader.GetDouble(11),
                EquipmentType = reader.GetString(12),
                MatchCount = reader.GetInt32(13),
                LastUpdated = DateTime.Parse(reader.GetString(14))
            };
            equipmentList.Add(equipment);
        }

        return equipmentList;
    }

    // ChampionData operations
    public async Task SaveChampionDataAsync(ChampionData championData, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT OR REPLACE INTO BigDataChampions (
                Id, Name, SetVersion, Cost, TraitsJson, ImageUrl, WinRate, PickRate, AveragePlacement, MatchCount, LastUpdated, CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @Name, @SetVersion, @Cost, @TraitsJson, @ImageUrl, @WinRate, @PickRate, @AveragePlacement, @MatchCount, @LastUpdated, @CreatedAt, @UpdatedAt
            );
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", championData.Id);
        command.Parameters.AddWithValue("@Name", championData.Name);
        command.Parameters.AddWithValue("@SetVersion", championData.SetVersion);
        command.Parameters.AddWithValue("@Cost", championData.Cost);
        command.Parameters.AddWithValue("@TraitsJson", JsonSerializer.Serialize(championData.Traits, _jsonOptions));
        command.Parameters.AddWithValue("@ImageUrl", championData.ImageUrl);
        command.Parameters.AddWithValue("@WinRate", championData.WinRate);
        command.Parameters.AddWithValue("@PickRate", championData.PickRate);
        command.Parameters.AddWithValue("@AveragePlacement", championData.AveragePlacement);
        command.Parameters.AddWithValue("@MatchCount", championData.MatchCount);
        command.Parameters.AddWithValue("@LastUpdated", championData.LastUpdated.ToString("O"));
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogDebug("Saved ChampionData with Id: {Id}", championData.Id);
    }

    public async Task<IEnumerable<ChampionData>> GetChampionDataAsync(string setVersion, int? cost = null, CancellationToken cancellationToken = default)
    {
        string sql = @"
            SELECT Id, Name, SetVersion, Cost, TraitsJson, ImageUrl, WinRate, PickRate, AveragePlacement, MatchCount, LastUpdated
            FROM BigDataChampions
            WHERE SetVersion = @SetVersion
        ";

        if (cost.HasValue)
        {
            sql += " AND Cost = @Cost";
        }

        sql += " ORDER BY WinRate DESC;";

        var champions = new List<ChampionData>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@SetVersion", setVersion);

        if (cost.HasValue)
        {
            command.Parameters.AddWithValue("@Cost", cost.Value);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var champion = new ChampionData
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                SetVersion = reader.GetString(2),
                Cost = reader.GetInt32(3),
                Traits = JsonSerializer.Deserialize<List<string>>(reader.GetString(4), _jsonOptions) ?? new List<string>(),
                ImageUrl = reader.GetString(5),
                WinRate = reader.GetDouble(6),
                PickRate = reader.GetDouble(7),
                AveragePlacement = reader.GetDouble(8),
                MatchCount = reader.GetInt32(9),
                LastUpdated = DateTime.Parse(reader.GetString(10))
            };
            champions.Add(champion);
        }

        return champions;
    }

    // Bulk operations
    public async Task SaveLineupDataBatchAsync(IEnumerable<LineupData> lineups, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var lineup in lineups)
            {
                await SaveLineupDataAsync(lineup, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger?.LogInformation("Bulk saved {Count} lineups", lineups.Count());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger?.LogError(ex, "Error saving lineup batch");
            throw;
        }
    }

    public async Task SaveEquipmentDataBatchAsync(IEnumerable<EquipmentData> equipmentList, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var equipment in equipmentList)
            {
                await SaveEquipmentDataAsync(equipment, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger?.LogInformation("Bulk saved {Count} equipment items", equipmentList.Count());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger?.LogError(ex, "Error saving equipment batch");
            throw;
        }
    }

    public async Task SaveChampionDataBatchAsync(IEnumerable<ChampionData> champions, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var champion in champions)
            {
                await SaveChampionDataAsync(champion, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger?.LogInformation("Bulk saved {Count} champions", champions.Count());
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger?.LogError(ex, "Error saving champion batch");
            throw;
        }
    }

    // Cleanup operations
    public async Task DeleteOldBigDataAsync(string setVersion, DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        var commands = new[]
        {
            "DELETE FROM BigDataMeta WHERE SetVersion = @SetVersion AND LastUpdated < @CutoffDate;",
            "DELETE FROM BigDataLineups WHERE SetVersion = @SetVersion AND LastUpdated < @CutoffDate;",
            "DELETE FROM BigDataEquipment WHERE SetVersion = @SetVersion AND LastUpdated < @CutoffDate;",
            "DELETE FROM BigDataChampions WHERE SetVersion = @SetVersion AND LastUpdated < @CutoffDate;"
        };

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        foreach (var sql in commands)
        {
            await using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@SetVersion", setVersion);
            command.Parameters.AddWithValue("@CutoffDate", cutoffDate.ToString("O"));
            var count = await command.ExecuteNonQueryAsync(cancellationToken);
            _logger?.LogInformation("Deleted {Count} records from BigData table", count);
        }
    }
}
