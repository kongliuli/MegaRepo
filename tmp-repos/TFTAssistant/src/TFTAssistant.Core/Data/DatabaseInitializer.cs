using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace TFTAssistant.Core.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseInitializer>? _logger;
    private const int CurrentSchemaVersion = 1;

    public DatabaseInitializer(string connectionString, ILogger<DatabaseInitializer>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Initializing database...");

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await CreateSchemaVersionTableAsync(connection, cancellationToken);
        var currentVersion = await GetCurrentSchemaVersionAsync(connection, cancellationToken);

        if (currentVersion < CurrentSchemaVersion)
        {
            _logger?.LogInformation("Migrating database from version {CurrentVersion} to {TargetVersion}", currentVersion, CurrentSchemaVersion);
            await MigrateAsync(connection, currentVersion, cancellationToken);
        }

        await CreateTablesAsync(connection, cancellationToken);

        _logger?.LogInformation("Database initialization complete.");
    }

    private async Task CreateSchemaVersionTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        const string sql = @"
            CREATE TABLE IF NOT EXISTS SchemaVersion (
                Version INTEGER PRIMARY KEY,
                AppliedAt TEXT NOT NULL
            );
        ";

        await using var command = new SqliteCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<int> GetCurrentSchemaVersionAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        const string sql = "SELECT Version FROM SchemaVersion ORDER BY Version DESC LIMIT 1;";

        await using var command = new SqliteCommand(sql, connection);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null ? Convert.ToInt32(result) : 0;
    }

    private async Task SetSchemaVersionAsync(SqliteConnection connection, int version, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO SchemaVersion (Version, AppliedAt)
            VALUES (@Version, @AppliedAt);
        ";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Version", version);
        command.Parameters.AddWithValue("@AppliedAt", DateTime.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task MigrateAsync(SqliteConnection connection, int fromVersion, CancellationToken cancellationToken)
    {
        for (int version = fromVersion + 1; version <= CurrentSchemaVersion; version++)
        {
            _logger?.LogInformation("Applying migration to version {Version}", version);
            switch (version)
            {
                case 1:
                    await CreateTablesAsync(connection, cancellationToken);
                    break;
            }
            await SetSchemaVersionAsync(connection, version, cancellationToken);
        }
    }

    private async Task CreateTablesAsync(SqliteConnection connection, CancellationToken cancellationToken)
        {
            var commands = new[]
            {
                // GameStates table
                @"
                CREATE TABLE IF NOT EXISTS GameStates (
                    Id TEXT PRIMARY KEY,
                    GameId TEXT NOT NULL,
                    Timestamp TEXT NOT NULL,
                    GameStateJson TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_GameStates_GameId ON GameStates(GameId);
                CREATE INDEX IF NOT EXISTS IX_GameStates_Timestamp ON GameStates(Timestamp);
                ",

                // MatchRecords table
                @"
                CREATE TABLE IF NOT EXISTS MatchRecords (
                    Id TEXT PRIMARY KEY,
                    GameId TEXT NOT NULL,
                    PlayerPuuid TEXT NOT NULL,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NOT NULL,
                    Placement INTEGER NOT NULL,
                    Level INTEGER NOT NULL,
                    GoldSpent INTEGER NOT NULL,
                    GoldRemaining INTEGER NOT NULL,
                    DamageDealt INTEGER NOT NULL,
                    DamageTaken INTEGER NOT NULL,
                    RoundsPlayed INTEGER NOT NULL,
                    SetVersion TEXT NOT NULL,
                    QueueType TEXT NOT NULL,
                    IsComplete INTEGER NOT NULL,
                    ChampionsJson TEXT NOT NULL,
                    TraitsJson TEXT NOT NULL,
                    ChampionStarsJson TEXT NOT NULL,
                    CompName TEXT NOT NULL,
                    CompId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_MatchRecords_PlayerPuuid ON MatchRecords(PlayerPuuid);
                CREATE INDEX IF NOT EXISTS IX_MatchRecords_StartTime ON MatchRecords(StartTime);
                CREATE INDEX IF NOT EXISTS IX_MatchRecords_CompId ON MatchRecords(CompId);
                ",

                // MatchDetails table
                @"
                CREATE TABLE IF NOT EXISTS MatchDetails (
                    Id TEXT PRIMARY KEY,
                    MatchRecordId TEXT NOT NULL,
                    BoardSnapshotsJson TEXT NOT NULL,
                    ShopSnapshotsJson TEXT NOT NULL,
                    AugmentSnapshotsJson TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (MatchRecordId) REFERENCES MatchRecords(Id) ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS IX_MatchDetails_MatchRecordId ON MatchDetails(MatchRecordId);
                ",

                // RoundSnapshots table
                @"
                CREATE TABLE IF NOT EXISTS RoundSnapshots (
                    Id TEXT PRIMARY KEY,
                    MatchRecordId TEXT NOT NULL,
                    RoundNumber INTEGER NOT NULL,
                    Stage TEXT NOT NULL,
                    Timestamp TEXT NOT NULL,
                    Level INTEGER NOT NULL,
                    Health INTEGER NOT NULL,
                    Gold INTEGER NOT NULL,
                    Experience INTEGER NOT NULL,
                    BoardUnitsJson TEXT NOT NULL,
                    BenchUnitsJson TEXT NOT NULL,
                    ActiveTraitsJson TEXT NOT NULL,
                    TraitCountsJson TEXT NOT NULL,
                    IsPvpRound INTEGER NOT NULL,
                    OpponentPlayerId TEXT,
                    OpponentHealth INTEGER,
                    DamageDealtThisRound INTEGER NOT NULL,
                    DamageTakenThisRound INTEGER NOT NULL,
                    GoldSpentThisRound INTEGER NOT NULL,
                    RefreshCountThisRound INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (MatchRecordId) REFERENCES MatchRecords(Id) ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS IX_RoundSnapshots_MatchRecordId ON RoundSnapshots(MatchRecordId);
                CREATE INDEX IF NOT EXISTS IX_RoundSnapshots_RoundNumber ON RoundSnapshots(RoundNumber);
                ",

                // BigData MetaData table
                @"
                CREATE TABLE IF NOT EXISTS BigDataMeta (
                    Id TEXT PRIMARY KEY,
                    Version TEXT NOT NULL,
                    SetVersion TEXT NOT NULL,
                    LastUpdated TEXT NOT NULL,
                    DataSource TEXT NOT NULL,
                    TotalMatches INTEGER NOT NULL,
                    TotalPlayers INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_BigDataMeta_Version ON BigDataMeta(Version);
                CREATE INDEX IF NOT EXISTS IX_BigDataMeta_SetVersion ON BigDataMeta(SetVersion);
                ",

                // BigData LineupData table
                @"
                CREATE TABLE IF NOT EXISTS BigDataLineups (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    SetVersion TEXT NOT NULL,
                    ChampionsJson TEXT NOT NULL,
                    TraitsJson TEXT NOT NULL,
                    ChampionItemsJson TEXT NOT NULL,
                    WinRate REAL NOT NULL,
                    PickRate REAL NOT NULL,
                    MatchCount INTEGER NOT NULL,
                    AveragePlacement INTEGER NOT NULL,
                    LastUpdated TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_BigDataLineups_SetVersion ON BigDataLineups(SetVersion);
                CREATE INDEX IF NOT EXISTS IX_BigDataLineups_WinRate ON BigDataLineups(WinRate);
                CREATE INDEX IF NOT EXISTS IX_BigDataLineups_PickRate ON BigDataLineups(PickRate);
                CREATE INDEX IF NOT EXISTS IX_BigDataLineups_LastUpdated ON BigDataLineups(LastUpdated);
                ",

                // BigData EquipmentData table
                @"
                CREATE TABLE IF NOT EXISTS BigDataEquipment (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    SetVersion TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    ImageUrl TEXT NOT NULL,
                    IsComponent INTEGER NOT NULL,
                    ComponentsJson TEXT NOT NULL,
                    WinRate REAL NOT NULL,
                    PickRate REAL NOT NULL,
                    MatchCount INTEGER NOT NULL,
                    LastUpdated TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_BigDataEquipment_SetVersion ON BigDataEquipment(SetVersion);
                CREATE INDEX IF NOT EXISTS IX_BigDataEquipment_IsComponent ON BigDataEquipment(IsComponent);
                CREATE INDEX IF NOT EXISTS IX_BigDataEquipment_WinRate ON BigDataEquipment(WinRate);
                CREATE INDEX IF NOT EXISTS IX_BigDataEquipment_LastUpdated ON BigDataEquipment(LastUpdated);
                ",

                // BigData ChampionData table
                @"
                CREATE TABLE IF NOT EXISTS BigDataChampions (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    SetVersion TEXT NOT NULL,
                    Cost INTEGER NOT NULL,
                    TraitsJson TEXT NOT NULL,
                    ImageUrl TEXT NOT NULL,
                    WinRate REAL NOT NULL,
                    PickRate REAL NOT NULL,
                    AveragePlacement REAL NOT NULL,
                    MatchCount INTEGER NOT NULL,
                    LastUpdated TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_BigDataChampions_SetVersion ON BigDataChampions(SetVersion);
                CREATE INDEX IF NOT EXISTS IX_BigDataChampions_Cost ON BigDataChampions(Cost);
                CREATE INDEX IF NOT EXISTS IX_BigDataChampions_WinRate ON BigDataChampions(WinRate);
                CREATE INDEX IF NOT EXISTS IX_BigDataChampions_PickRate ON BigDataChampions(PickRate);
                CREATE INDEX IF NOT EXISTS IX_BigDataChampions_LastUpdated ON BigDataChampions(LastUpdated);
                ",

                // BigDataMeta additional indexes
                @"
                CREATE INDEX IF NOT EXISTS IX_BigDataMeta_LastUpdated ON BigDataMeta(LastUpdated);
                CREATE INDEX IF NOT EXISTS IX_BigDataMeta_SetVersion_LastUpdated ON BigDataMeta(SetVersion, LastUpdated);
                "
            };

            foreach (var sql in commands)
            {
                await using var command = new SqliteCommand(sql, connection);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
}
