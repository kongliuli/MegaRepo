using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Analytics;

namespace TFTAssistant.Core.Data;

public class SqliteMatchRepository : IMatchRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteMatchRepository>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SqliteMatchRepository(string connectionString, ILogger<SqliteMatchRepository>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task SaveMatchRecordAsync(MatchRecord matchRecord, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT OR REPLACE INTO MatchRecords (
                Id, GameId, PlayerPuuid, StartTime, EndTime, Placement, Level, GoldSpent,
                GoldRemaining, DamageDealt, DamageTaken, RoundsPlayed, SetVersion, QueueType,
                IsComplete, ChampionsJson, TraitsJson, ChampionStarsJson, CompName, CompId,
                CreatedAt, UpdatedAt
            ) VALUES (
                @Id, @GameId, @PlayerPuuid, @StartTime, @EndTime, @Placement, @Level, @GoldSpent,
                @GoldRemaining, @DamageDealt, @DamageTaken, @RoundsPlayed, @SetVersion, @QueueType,
                @IsComplete, @ChampionsJson, @TraitsJson, @ChampionStarsJson, @CompName, @CompId,
                @CreatedAt, @UpdatedAt
            );
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", matchRecord.Id);
        command.Parameters.AddWithValue("@GameId", matchRecord.GameId);
        command.Parameters.AddWithValue("@PlayerPuuid", matchRecord.PlayerPuuid);
        command.Parameters.AddWithValue("@StartTime", matchRecord.StartTime.ToString("O"));
        command.Parameters.AddWithValue("@EndTime", matchRecord.EndTime.ToString("O"));
        command.Parameters.AddWithValue("@Placement", matchRecord.Placement);
        command.Parameters.AddWithValue("@Level", matchRecord.Level);
        command.Parameters.AddWithValue("@GoldSpent", matchRecord.GoldSpent);
        command.Parameters.AddWithValue("@GoldRemaining", matchRecord.GoldRemaining);
        command.Parameters.AddWithValue("@DamageDealt", matchRecord.DamageDealt);
        command.Parameters.AddWithValue("@DamageTaken", matchRecord.DamageTaken);
        command.Parameters.AddWithValue("@RoundsPlayed", matchRecord.RoundsPlayed);
        command.Parameters.AddWithValue("@SetVersion", matchRecord.SetVersion);
        command.Parameters.AddWithValue("@QueueType", matchRecord.QueueType);
        command.Parameters.AddWithValue("@IsComplete", matchRecord.IsComplete ? 1 : 0);
        command.Parameters.AddWithValue("@ChampionsJson", JsonSerializer.Serialize(matchRecord.Champions, _jsonOptions));
        command.Parameters.AddWithValue("@TraitsJson", JsonSerializer.Serialize(matchRecord.Traits, _jsonOptions));
        command.Parameters.AddWithValue("@ChampionStarsJson", JsonSerializer.Serialize(matchRecord.ChampionStars, _jsonOptions));
        command.Parameters.AddWithValue("@CompName", matchRecord.CompName);
        command.Parameters.AddWithValue("@CompId", matchRecord.CompId);
        command.Parameters.AddWithValue("@CreatedAt", matchRecord.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("@UpdatedAt", matchRecord.UpdatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogDebug("Saved MatchRecord with Id: {Id}", matchRecord.Id);
    }

    public async Task<MatchRecord?> GetMatchRecordByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, GameId, PlayerPuuid, StartTime, EndTime, Placement, Level, GoldSpent,
                   GoldRemaining, DamageDealt, DamageTaken, RoundsPlayed, SetVersion, QueueType,
                   IsComplete, ChampionsJson, TraitsJson, ChampionStarsJson, CompName, CompId,
                   CreatedAt, UpdatedAt
            FROM MatchRecords
            WHERE Id = @Id;
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return MapToMatchRecord(reader);
        }

        return null;
    }

    public async Task<IEnumerable<MatchRecord>> GetMatchRecordsAsync(int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, GameId, PlayerPuuid, StartTime, EndTime, Placement, Level, GoldSpent,
                   GoldRemaining, DamageDealt, DamageTaken, RoundsPlayed, SetVersion, QueueType,
                   IsComplete, ChampionsJson, TraitsJson, ChampionStarsJson, CompName, CompId,
                   CreatedAt, UpdatedAt
            FROM MatchRecords
            ORDER BY StartTime DESC
            LIMIT @Limit OFFSET @Offset;
        ";

        var matchRecords = new List<MatchRecord>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Limit", limit);
        command.Parameters.AddWithValue("@Offset", offset);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var matchRecord = MapToMatchRecord(reader);
            if (matchRecord != null)
            {
                matchRecords.Add(matchRecord);
            }
        }

        return matchRecords;
    }

    public async Task<IEnumerable<MatchRecord>> GetMatchRecordsByPlayerAsync(string playerPuuid, int limit = 100, int offset = 0, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, GameId, PlayerPuuid, StartTime, EndTime, Placement, Level, GoldSpent,
                   GoldRemaining, DamageDealt, DamageTaken, RoundsPlayed, SetVersion, QueueType,
                   IsComplete, ChampionsJson, TraitsJson, ChampionStarsJson, CompName, CompId,
                   CreatedAt, UpdatedAt
            FROM MatchRecords
            WHERE PlayerPuuid = @PlayerPuuid
            ORDER BY StartTime DESC
            LIMIT @Limit OFFSET @Offset;
        ";

        var matchRecords = new List<MatchRecord>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@PlayerPuuid", playerPuuid);
        command.Parameters.AddWithValue("@Limit", limit);
        command.Parameters.AddWithValue("@Offset", offset);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var matchRecord = MapToMatchRecord(reader);
            if (matchRecord != null)
            {
                matchRecords.Add(matchRecord);
            }
        }

        return matchRecords;
    }

    public async Task<IEnumerable<MatchRecord>> GetMatchRecordsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, GameId, PlayerPuuid, StartTime, EndTime, Placement, Level, GoldSpent,
                   GoldRemaining, DamageDealt, DamageTaken, RoundsPlayed, SetVersion, QueueType,
                   IsComplete, ChampionsJson, TraitsJson, ChampionStarsJson, CompName, CompId,
                   CreatedAt, UpdatedAt
            FROM MatchRecords
            WHERE StartTime >= @StartDate AND StartTime <= @EndDate
            ORDER BY StartTime DESC;
        ";

        var matchRecords = new List<MatchRecord>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@StartDate", startDate.ToString("O"));
        command.Parameters.AddWithValue("@EndDate", endDate.ToString("O"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var matchRecord = MapToMatchRecord(reader);
            if (matchRecord != null)
            {
                matchRecords.Add(matchRecord);
            }
        }

        return matchRecords;
    }

    public async Task<MatchStats?> GetMatchStatsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                COUNT(*) as TotalMatches,
                SUM(CASE WHEN Placement = 1 THEN 1 ELSE 0 END) as Wins,
                SUM(CASE WHEN Placement <= 4 THEN 1 ELSE 0 END) as Top4,
                SUM(CASE WHEN Placement <= 8 THEN 1 ELSE 0 END) as Top8,
                AVG(Placement) as AveragePlacement,
                AVG(Level) as AverageLevel,
                AVG(GoldSpent) as AverageGoldSpent,
                AVG(RoundsPlayed) as AverageRoundsPlayed
            FROM MatchRecords
            WHERE PlayerPuuid = @PlayerPuuid
        ";

        if (startDate.HasValue)
        {
            sql += " AND StartTime >= @StartDate";
        }
        if (endDate.HasValue)
        {
            sql += " AND StartTime <= @EndDate";
        }

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@PlayerPuuid", playerPuuid);
        if (startDate.HasValue)
        {
            command.Parameters.AddWithValue("@StartDate", startDate.Value.ToString("O"));
        }
        if (endDate.HasValue)
        {
            command.Parameters.AddWithValue("@EndDate", endDate.Value.ToString("O"));
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var totalMatches = reader.GetInt32(0);
            if (totalMatches == 0)
            {
                return null;
            }

            var wins = reader.GetInt32(1);
            var top4 = reader.GetInt32(2);
            var top8 = reader.GetInt32(3);

            return new MatchStats
            {
                Id = Guid.NewGuid().ToString(),
                PlayerPuuid = playerPuuid,
                PeriodStart = startDate ?? DateTime.MinValue,
                PeriodEnd = endDate ?? DateTime.UtcNow,
                TotalMatches = totalMatches,
                Wins = wins,
                Top4 = top4,
                Top8 = top8,
                WinRate = totalMatches > 0 ? (double)wins / totalMatches : 0,
                Top4Rate = totalMatches > 0 ? (double)top4 / totalMatches : 0,
                AveragePlacement = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                AverageLevel = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                AverageGoldSpent = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                AverageRoundsPlayed = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsHistoricalData = true,
                DataSourceNote = "历史统计数据，仅供参考"
            };
        }

        return null;
    }

    public async Task<IEnumerable<CompStats>> GetCompStatsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                CompId,
                CompName,
                COUNT(*) as TotalGames,
                SUM(CASE WHEN Placement = 1 THEN 1 ELSE 0 END) as Wins,
                SUM(CASE WHEN Placement <= 4 THEN 1 ELSE 0 END) as Top4,
                AVG(Placement) as AveragePlacement,
                AVG(Level) as AverageLevel,
                AVG(RoundsPlayed) as AverageRoundsPlayed
            FROM MatchRecords
            WHERE PlayerPuuid = @PlayerPuuid AND CompId != ''
        ";

        if (startDate.HasValue)
        {
            sql += " AND StartTime >= @StartDate";
        }
        if (endDate.HasValue)
        {
            sql += " AND StartTime <= @EndDate";
        }

        sql += " GROUP BY CompId, CompName ORDER BY TotalGames DESC;";

        var compStatsList = new List<CompStats>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@PlayerPuuid", playerPuuid);
        if (startDate.HasValue)
        {
            command.Parameters.AddWithValue("@StartDate", startDate.Value.ToString("O"));
        }
        if (endDate.HasValue)
        {
            command.Parameters.AddWithValue("@EndDate", endDate.Value.ToString("O"));
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var totalGames = reader.GetInt32(2);
            if (totalGames == 0) continue;

            var wins = reader.GetInt32(3);
            var top4 = reader.GetInt32(4);

            compStatsList.Add(new CompStats
            {
                Id = Guid.NewGuid().ToString(),
                CompId = reader.GetString(0),
                CompName = reader.GetString(1),
                PlayerPuuid = playerPuuid,
                PeriodStart = startDate ?? DateTime.MinValue,
                PeriodEnd = endDate ?? DateTime.UtcNow,
                TotalGames = totalGames,
                Wins = wins,
                Top4 = top4,
                WinRate = (double)wins / totalGames,
                Top4Rate = (double)top4 / totalGames,
                AveragePlacement = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                AverageLevel = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                AverageRoundsPlayed = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsHistoricalData = true,
                DataSourceNote = "历史统计数据，仅供参考"
            });
        }

        return compStatsList;
    }

    public async Task<IEnumerable<VersionStats>> GetVersionStatsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT 
                SetVersion,
                COUNT(*) as TotalMatches,
                SUM(CASE WHEN Placement = 1 THEN 1 ELSE 0 END) as Wins,
                SUM(CASE WHEN Placement <= 4 THEN 1 ELSE 0 END) as Top4,
                SUM(CASE WHEN Placement <= 8 THEN 1 ELSE 0 END) as Top8,
                AVG(Placement) as AveragePlacement,
                AVG(Level) as AverageLevel,
                AVG(GoldSpent) as AverageGoldSpent,
                AVG(RoundsPlayed) as AverageRoundsPlayed
            FROM MatchRecords
            WHERE PlayerPuuid = @PlayerPuuid AND SetVersion != ''
        ";

        if (startDate.HasValue)
        {
            sql += " AND StartTime >= @StartDate";
        }
        if (endDate.HasValue)
        {
            sql += " AND StartTime <= @EndDate";
        }

        sql += " GROUP BY SetVersion ORDER BY TotalMatches DESC;";

        var versionStatsList = new List<VersionStats>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@PlayerPuuid", playerPuuid);
        if (startDate.HasValue)
        {
            command.Parameters.AddWithValue("@StartDate", startDate.Value.ToString("O"));
        }
        if (endDate.HasValue)
        {
            command.Parameters.AddWithValue("@EndDate", endDate.Value.ToString("O"));
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var totalMatches = reader.GetInt32(1);
            if (totalMatches == 0) continue;

            var wins = reader.GetInt32(2);
            var top4 = reader.GetInt32(3);
            var top8 = reader.GetInt32(4);

            versionStatsList.Add(new VersionStats
            {
                Id = Guid.NewGuid().ToString(),
                SetVersion = reader.GetString(0),
                PlayerPuuid = playerPuuid,
                PeriodStart = startDate ?? DateTime.MinValue,
                PeriodEnd = endDate ?? DateTime.UtcNow,
                TotalMatches = totalMatches,
                Wins = wins,
                Top4 = top4,
                Top8 = top8,
                WinRate = (double)wins / totalMatches,
                Top4Rate = (double)top4 / totalMatches,
                AveragePlacement = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                AverageLevel = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                AverageGoldSpent = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                AverageRoundsPlayed = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsHistoricalData = true,
                DataSourceNote = "历史统计数据，仅供参考"
            });
        }

        return versionStatsList;
    }

    public async Task SaveMatchDetailAsync(MatchDetail matchDetail, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT OR REPLACE INTO MatchDetails (
                Id, MatchRecordId, BoardSnapshotsJson, ShopSnapshotsJson, AugmentSnapshotsJson, CreatedAt
            ) VALUES (
                @Id, @MatchRecordId, @BoardSnapshotsJson, @ShopSnapshotsJson, @AugmentSnapshotsJson, @CreatedAt
            );
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", matchDetail.Id);
        command.Parameters.AddWithValue("@MatchRecordId", matchDetail.MatchRecordId);
        command.Parameters.AddWithValue("@BoardSnapshotsJson", JsonSerializer.Serialize(matchDetail.BoardSnapshots, _jsonOptions));
        command.Parameters.AddWithValue("@ShopSnapshotsJson", JsonSerializer.Serialize(matchDetail.ShopSnapshots, _jsonOptions));
        command.Parameters.AddWithValue("@AugmentSnapshotsJson", JsonSerializer.Serialize(matchDetail.AugmentSnapshots, _jsonOptions));
        command.Parameters.AddWithValue("@CreatedAt", matchDetail.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogDebug("Saved MatchDetail with Id: {Id}", matchDetail.Id);
    }

    public async Task<MatchDetail?> GetMatchDetailByMatchIdAsync(string matchRecordId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, MatchRecordId, BoardSnapshotsJson, ShopSnapshotsJson, AugmentSnapshotsJson, CreatedAt
            FROM MatchDetails
            WHERE MatchRecordId = @MatchRecordId;
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@MatchRecordId", matchRecordId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new MatchDetail
            {
                Id = reader.GetString(0),
                MatchRecordId = reader.GetString(1),
                BoardSnapshots = JsonSerializer.Deserialize<ImmutableList<BoardSnapshot>>(reader.GetString(2), _jsonOptions) ?? ImmutableList<BoardSnapshot>.Empty,
                ShopSnapshots = JsonSerializer.Deserialize<ImmutableList<ShopSnapshot>>(reader.GetString(3), _jsonOptions) ?? ImmutableList<ShopSnapshot>.Empty,
                AugmentSnapshots = JsonSerializer.Deserialize<ImmutableList<AugmentSnapshot>>(reader.GetString(4), _jsonOptions) ?? ImmutableList<AugmentSnapshot>.Empty,
                CreatedAt = DateTime.Parse(reader.GetString(5))
            };
        }

        return null;
    }

    public async Task SaveRoundSnapshotAsync(RoundSnapshot roundSnapshot, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT OR REPLACE INTO RoundSnapshots (
                Id, MatchRecordId, RoundNumber, Stage, Timestamp, Level, Health, Gold, Experience,
                BoardUnitsJson, BenchUnitsJson, ActiveTraitsJson, TraitCountsJson, IsPvpRound,
                OpponentPlayerId, OpponentHealth, DamageDealtThisRound, DamageTakenThisRound,
                GoldSpentThisRound, RefreshCountThisRound, CreatedAt
            ) VALUES (
                @Id, @MatchRecordId, @RoundNumber, @Stage, @Timestamp, @Level, @Health, @Gold, @Experience,
                @BoardUnitsJson, @BenchUnitsJson, @ActiveTraitsJson, @TraitCountsJson, @IsPvpRound,
                @OpponentPlayerId, @OpponentHealth, @DamageDealtThisRound, @DamageTakenThisRound,
                @GoldSpentThisRound, @RefreshCountThisRound, @CreatedAt
            );
        ";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", roundSnapshot.Id);
        command.Parameters.AddWithValue("@MatchRecordId", roundSnapshot.MatchRecordId);
        command.Parameters.AddWithValue("@RoundNumber", roundSnapshot.RoundNumber);
        command.Parameters.AddWithValue("@Stage", roundSnapshot.Stage);
        command.Parameters.AddWithValue("@Timestamp", roundSnapshot.Timestamp.ToString("O"));
        command.Parameters.AddWithValue("@Level", roundSnapshot.Level);
        command.Parameters.AddWithValue("@Health", roundSnapshot.Health);
        command.Parameters.AddWithValue("@Gold", roundSnapshot.Gold);
        command.Parameters.AddWithValue("@Experience", roundSnapshot.Experience);
        command.Parameters.AddWithValue("@BoardUnitsJson", JsonSerializer.Serialize(roundSnapshot.BoardUnits, _jsonOptions));
        command.Parameters.AddWithValue("@BenchUnitsJson", JsonSerializer.Serialize(roundSnapshot.BenchUnits, _jsonOptions));
        command.Parameters.AddWithValue("@ActiveTraitsJson", JsonSerializer.Serialize(roundSnapshot.ActiveTraits, _jsonOptions));
        command.Parameters.AddWithValue("@TraitCountsJson", JsonSerializer.Serialize(roundSnapshot.TraitCounts, _jsonOptions));
        command.Parameters.AddWithValue("@IsPvpRound", roundSnapshot.IsPvpRound ? 1 : 0);
        command.Parameters.AddWithValue("@OpponentPlayerId", roundSnapshot.OpponentPlayerId ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@OpponentHealth", roundSnapshot.OpponentHealth ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DamageDealtThisRound", roundSnapshot.DamageDealtThisRound);
        command.Parameters.AddWithValue("@DamageTakenThisRound", roundSnapshot.DamageTakenThisRound);
        command.Parameters.AddWithValue("@GoldSpentThisRound", roundSnapshot.GoldSpentThisRound);
        command.Parameters.AddWithValue("@RefreshCountThisRound", roundSnapshot.RefreshCountThisRound);
        command.Parameters.AddWithValue("@CreatedAt", roundSnapshot.CreatedAt.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogDebug("Saved RoundSnapshot with Id: {Id}", roundSnapshot.Id);
    }

    public async Task<IEnumerable<RoundSnapshot>> GetRoundSnapshotsByMatchIdAsync(string matchRecordId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, MatchRecordId, RoundNumber, Stage, Timestamp, Level, Health, Gold, Experience,
                   BoardUnitsJson, BenchUnitsJson, ActiveTraitsJson, TraitCountsJson, IsPvpRound,
                   OpponentPlayerId, OpponentHealth, DamageDealtThisRound, DamageTakenThisRound,
                   GoldSpentThisRound, RefreshCountThisRound, CreatedAt
            FROM RoundSnapshots
            WHERE MatchRecordId = @MatchRecordId
            ORDER BY RoundNumber ASC;
        ";

        var roundSnapshots = new List<RoundSnapshot>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@MatchRecordId", matchRecordId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            roundSnapshots.Add(new RoundSnapshot
            {
                Id = reader.GetString(0),
                MatchRecordId = reader.GetString(1),
                RoundNumber = reader.GetInt32(2),
                Stage = reader.GetString(3),
                Timestamp = DateTime.Parse(reader.GetString(4)),
                Level = reader.GetInt32(5),
                Health = reader.GetInt32(6),
                Gold = reader.GetInt32(7),
                Experience = reader.GetInt32(8),
                BoardUnits = JsonSerializer.Deserialize<ImmutableList<UnitSnapshot>>(reader.GetString(9), _jsonOptions) ?? ImmutableList<UnitSnapshot>.Empty,
                BenchUnits = JsonSerializer.Deserialize<ImmutableList<UnitSnapshot>>(reader.GetString(10), _jsonOptions) ?? ImmutableList<UnitSnapshot>.Empty,
                ActiveTraits = JsonSerializer.Deserialize<ImmutableList<string>>(reader.GetString(11), _jsonOptions) ?? ImmutableList<string>.Empty,
                TraitCounts = JsonSerializer.Deserialize<ImmutableDictionary<string, int>>(reader.GetString(12), _jsonOptions) ?? ImmutableDictionary<string, int>.Empty,
                IsPvpRound = reader.GetInt32(13) == 1,
                OpponentPlayerId = reader.IsDBNull(14) ? null : reader.GetString(14),
                OpponentHealth = reader.IsDBNull(15) ? null : reader.GetInt32(15),
                DamageDealtThisRound = reader.GetInt32(16),
                DamageTakenThisRound = reader.GetInt32(17),
                GoldSpentThisRound = reader.GetInt32(18),
                RefreshCountThisRound = reader.GetInt32(19),
                CreatedAt = DateTime.Parse(reader.GetString(20))
            });
        }

        return roundSnapshots;
    }

    public async Task DeleteMatchRecordAsync(string id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM MatchRecords WHERE Id = @Id;";

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        var count = await command.ExecuteNonQueryAsync(cancellationToken);
        _logger?.LogInformation("Deleted {Count} MatchRecord with Id: {Id}", count, id);
    }

    private MatchRecord? MapToMatchRecord(SqliteDataReader reader)
    {
        try
        {
            return new MatchRecord
            {
                Id = reader.GetString(0),
                GameId = reader.GetString(1),
                PlayerPuuid = reader.GetString(2),
                StartTime = DateTime.Parse(reader.GetString(3)),
                EndTime = DateTime.Parse(reader.GetString(4)),
                Placement = reader.GetInt32(5),
                Level = reader.GetInt32(6),
                GoldSpent = reader.GetInt32(7),
                GoldRemaining = reader.GetInt32(8),
                DamageDealt = reader.GetInt32(9),
                DamageTaken = reader.GetInt32(10),
                RoundsPlayed = reader.GetInt32(11),
                SetVersion = reader.GetString(12),
                QueueType = reader.GetString(13),
                IsComplete = reader.GetInt32(14) == 1,
                Champions = JsonSerializer.Deserialize<ImmutableList<string>>(reader.GetString(15), _jsonOptions) ?? ImmutableList<string>.Empty,
                Traits = JsonSerializer.Deserialize<ImmutableList<string>>(reader.GetString(16), _jsonOptions) ?? ImmutableList<string>.Empty,
                ChampionStars = JsonSerializer.Deserialize<ImmutableDictionary<string, int>>(reader.GetString(17), _jsonOptions) ?? ImmutableDictionary<string, int>.Empty,
                CompName = reader.GetString(18),
                CompId = reader.GetString(19),
                CreatedAt = DateTime.Parse(reader.GetString(20)),
                UpdatedAt = DateTime.Parse(reader.GetString(21))
            };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error mapping MatchRecord from database");
            return null;
        }
    }
}
