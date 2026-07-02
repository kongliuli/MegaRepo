using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.Analytics;

namespace TFTAssistant.Core.Tests;

public class SqliteMatchRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    private readonly Mock<ILogger<SqliteMatchRepository>> _mockLogger;
    private readonly SqliteMatchRepository _repository;

    public SqliteMatchRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath}";
        _mockLogger = new Mock<ILogger<SqliteMatchRepository>>();
        _repository = new SqliteMatchRepository(_connectionString, _mockLogger.Object);

        InitializeDatabaseAsync().Wait();
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        var initializer = new DatabaseInitializer(_connectionString);
        await initializer.InitializeAsync();
    }

    private MatchRecord CreateTestMatchRecord(string playerPuuid = "test-puuid-123", int placement = 1)
    {
        return new MatchRecord
        {
            Id = Guid.NewGuid().ToString(),
            GameId = "game-123",
            PlayerPuuid = playerPuuid,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            Placement = placement,
            Level = 8,
            GoldSpent = 200,
            GoldRemaining = 10,
            DamageDealt = 500,
            DamageTaken = 300,
            RoundsPlayed = 30,
            SetVersion = "15.1",
            QueueType = "Ranked",
            IsComplete = true,
            Champions = ImmutableList<string>.Empty.Add("Ahri").Add("Ezreal"),
            Traits = ImmutableList<string>.Empty.Add("Arcanist").Add("Inkborn"),
            ChampionStars = ImmutableDictionary<string, int>.Empty.Add("Ahri", 2).Add("Ezreal", 3),
            CompName = "Arcanist Inkborn",
            CompId = "arcanist-inkborn",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        _repository.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveMatchRecordAsync_ShouldSaveSuccessfully()
    {
        var matchRecord = CreateTestMatchRecord();

        var action = () => _repository.SaveMatchRecordAsync(matchRecord);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveAndGetMatchRecordAsync_ShouldReturnSameRecord()
    {
        var matchRecord = CreateTestMatchRecord();
        await _repository.SaveMatchRecordAsync(matchRecord);

        var retrieved = await _repository.GetMatchRecordByIdAsync(matchRecord.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(matchRecord.Id);
        retrieved.PlayerPuuid.Should().Be(matchRecord.PlayerPuuid);
        retrieved.Placement.Should().Be(matchRecord.Placement);
    }

    [Fact]
    public async Task GetMatchRecordByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var result = await _repository.GetMatchRecordByIdAsync("invalid-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMatchRecordsAsync_WithNoData_ShouldReturnEmpty()
    {
        var result = (await _repository.GetMatchRecordsAsync()).ToList();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMatchRecordsAsync_WithData_ShouldReturnRecords()
    {
        var record1 = CreateTestMatchRecord("player-1");
        var record2 = CreateTestMatchRecord("player-2");
        await _repository.SaveMatchRecordAsync(record1);
        await _repository.SaveMatchRecordAsync(record2);

        var result = (await _repository.GetMatchRecordsAsync()).ToList();

        result.Should().NotBeNull();
        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetMatchRecordsByPlayerAsync_ShouldReturnPlayerRecords()
    {
        var targetPlayer = "target-player";
        var record1 = CreateTestMatchRecord(targetPlayer);
        var record2 = CreateTestMatchRecord("other-player");
        await _repository.SaveMatchRecordAsync(record1);
        await _repository.SaveMatchRecordAsync(record2);

        var result = (await _repository.GetMatchRecordsByPlayerAsync(targetPlayer)).ToList();

        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].PlayerPuuid.Should().Be(targetPlayer);
    }

    [Fact]
    public async Task GetMatchStatsAsync_WithMatches_ShouldReturnStats()
    {
        var playerPuuid = "stats-player";
        var record1 = CreateTestMatchRecord(playerPuuid, 1);
        var record2 = CreateTestMatchRecord(playerPuuid, 3);
        var record3 = CreateTestMatchRecord(playerPuuid, 5);
        await _repository.SaveMatchRecordAsync(record1);
        await _repository.SaveMatchRecordAsync(record2);
        await _repository.SaveMatchRecordAsync(record3);

        var stats = await _repository.GetMatchStatsAsync(playerPuuid);

        stats.Should().NotBeNull();
        stats!.TotalMatches.Should().Be(3);
        stats.Wins.Should().Be(1);
        stats.Top4.Should().Be(2);
        stats.WinRate.Should().BeApproximately(1.0 / 3.0, 0.001);
    }

    [Fact]
    public async Task GetMatchStatsAsync_WithNoMatches_ShouldReturnNull()
    {
        var stats = await _repository.GetMatchStatsAsync("no-matches-player");

        stats.Should().BeNull();
    }

    [Fact]
    public async Task GetCompStatsAsync_WithMatches_ShouldReturnCompStats()
    {
        var playerPuuid = "comp-stats-player";
        var record1 = CreateTestMatchRecord(playerPuuid, 1);
        record1.CompId = "comp-1";
        var record2 = CreateTestMatchRecord(playerPuuid, 2);
        record2.CompId = "comp-1";
        var record3 = CreateTestMatchRecord(playerPuuid, 5);
        record3.CompId = "comp-2";
        await _repository.SaveMatchRecordAsync(record1);
        await _repository.SaveMatchRecordAsync(record2);
        await _repository.SaveMatchRecordAsync(record3);

        var stats = (await _repository.GetCompStatsAsync(playerPuuid)).ToList();

        stats.Should().NotBeNull();
        stats.Count.Should().Be(2);
        stats[0].TotalGames.Should().Be(2);
        stats[0].Wins.Should().Be(1);
    }

    [Fact]
    public async Task GetVersionStatsAsync_WithMatches_ShouldReturnVersionStats()
    {
        var playerPuuid = "version-stats-player";
        var record1 = CreateTestMatchRecord(playerPuuid, 1);
        record1.SetVersion = "15.1";
        var record2 = CreateTestMatchRecord(playerPuuid, 2);
        record2.SetVersion = "15.1";
        var record3 = CreateTestMatchRecord(playerPuuid, 3);
        record3.SetVersion = "14.23";
        await _repository.SaveMatchRecordAsync(record1);
        await _repository.SaveMatchRecordAsync(record2);
        await _repository.SaveMatchRecordAsync(record3);

        var stats = (await _repository.GetVersionStatsAsync(playerPuuid)).ToList();

        stats.Should().NotBeNull();
        stats.Count.Should().Be(2);
        stats[0].TotalMatches.Should().Be(2);
        stats[0].SetVersion.Should().Be("15.1");
    }

    [Fact]
    public async Task DeleteMatchRecordAsync_ShouldDeleteRecord()
    {
        var record = CreateTestMatchRecord();
        await _repository.SaveMatchRecordAsync(record);

        await _repository.DeleteMatchRecordAsync(record.Id);

        var retrieved = await _repository.GetMatchRecordByIdAsync(record.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task SaveMatchDetailAsync_ShouldSaveSuccessfully()
    {
        var matchRecord = CreateTestMatchRecord();
        await _repository.SaveMatchRecordAsync(matchRecord);

        var matchDetail = new MatchDetail
        {
            Id = Guid.NewGuid().ToString(),
            MatchRecordId = matchRecord.Id,
            BoardSnapshots = ImmutableList<BoardSnapshot>.Empty,
            ShopSnapshots = ImmutableList<ShopSnapshot>.Empty,
            AugmentSnapshots = ImmutableList<AugmentSnapshot>.Empty,
            CreatedAt = DateTime.UtcNow
        };

        var action = () => _repository.SaveMatchDetailAsync(matchDetail);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveAndGetMatchDetailAsync_ShouldReturnSameDetail()
    {
        var matchRecord = CreateTestMatchRecord();
        await _repository.SaveMatchRecordAsync(matchRecord);

        var matchDetail = new MatchDetail
        {
            Id = Guid.NewGuid().ToString(),
            MatchRecordId = matchRecord.Id,
            BoardSnapshots = ImmutableList<BoardSnapshot>.Empty,
            ShopSnapshots = ImmutableList<ShopSnapshot>.Empty,
            AugmentSnapshots = ImmutableList<AugmentSnapshot>.Empty,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.SaveMatchDetailAsync(matchDetail);

        var retrieved = await _repository.GetMatchDetailByMatchIdAsync(matchRecord.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(matchDetail.Id);
        retrieved.MatchRecordId.Should().Be(matchRecord.Id);
    }

    [Fact]
    public async Task SaveRoundSnapshotAsync_ShouldSaveSuccessfully()
    {
        var matchRecord = CreateTestMatchRecord();
        await _repository.SaveMatchRecordAsync(matchRecord);

        var roundSnapshot = new RoundSnapshot
        {
            Id = Guid.NewGuid().ToString(),
            MatchRecordId = matchRecord.Id,
            RoundNumber = 5,
            Stage = "2-1",
            Timestamp = DateTime.UtcNow,
            Level = 5,
            Health = 100,
            Gold = 50,
            Experience = 0,
            BoardUnits = ImmutableList<UnitSnapshot>.Empty,
            BenchUnits = ImmutableList<UnitSnapshot>.Empty,
            ActiveTraits = ImmutableList<string>.Empty,
            TraitCounts = ImmutableDictionary<string, int>.Empty,
            IsPvpRound = false,
            DamageDealtThisRound = 0,
            DamageTakenThisRound = 0,
            GoldSpentThisRound = 0,
            RefreshCountThisRound = 0,
            CreatedAt = DateTime.UtcNow
        };

        var action = () => _repository.SaveRoundSnapshotAsync(roundSnapshot);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveAndGetRoundSnapshotsAsync_ShouldReturnSnapshots()
    {
        var matchRecord = CreateTestMatchRecord();
        await _repository.SaveMatchRecordAsync(matchRecord);

        var snapshot1 = new RoundSnapshot
        {
            Id = Guid.NewGuid().ToString(),
            MatchRecordId = matchRecord.Id,
            RoundNumber = 1,
            Stage = "1-1",
            Timestamp = DateTime.UtcNow.AddMinutes(-10),
            Level = 2,
            Health = 100,
            Gold = 2,
            Experience = 0,
            BoardUnits = ImmutableList<UnitSnapshot>.Empty,
            BenchUnits = ImmutableList<UnitSnapshot>.Empty,
            ActiveTraits = ImmutableList<string>.Empty,
            TraitCounts = ImmutableDictionary<string, int>.Empty,
            IsPvpRound = false,
            DamageDealtThisRound = 0,
            DamageTakenThisRound = 0,
            GoldSpentThisRound = 0,
            RefreshCountThisRound = 0,
            CreatedAt = DateTime.UtcNow
        };

        var snapshot2 = new RoundSnapshot
        {
            Id = Guid.NewGuid().ToString(),
            MatchRecordId = matchRecord.Id,
            RoundNumber = 2,
            Stage = "1-2",
            Timestamp = DateTime.UtcNow.AddMinutes(-5),
            Level = 3,
            Health = 100,
            Gold = 5,
            Experience = 0,
            BoardUnits = ImmutableList<UnitSnapshot>.Empty,
            BenchUnits = ImmutableList<UnitSnapshot>.Empty,
            ActiveTraits = ImmutableList<string>.Empty,
            TraitCounts = ImmutableDictionary<string, int>.Empty,
            IsPvpRound = false,
            DamageDealtThisRound = 0,
            DamageTakenThisRound = 0,
            GoldSpentThisRound = 0,
            RefreshCountThisRound = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.SaveRoundSnapshotAsync(snapshot1);
        await _repository.SaveRoundSnapshotAsync(snapshot2);

        var retrieved = (await _repository.GetRoundSnapshotsByMatchIdAsync(matchRecord.Id)).ToList();

        retrieved.Should().NotBeNull();
        retrieved.Count.Should().Be(2);
        retrieved[0].RoundNumber.Should().Be(1);
        retrieved[1].RoundNumber.Should().Be(2);
    }

    [Fact]
    public void Constructor_WithoutLogger_ShouldStillWork()
    {
        var action = () => new SqliteMatchRepository(_connectionString);

        action.Should().NotThrow();
    }
}
