using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Tests;

public class SqliteGameStateRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;
    private readonly Mock<ILogger<SqliteGameStateRepository>> _mockLogger;
    private readonly SqliteGameStateRepository _repository;

    public SqliteGameStateRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath}";
        _mockLogger = new Mock<ILogger<SqliteGameStateRepository>>();
        _repository = new SqliteGameStateRepository(_connectionString, _mockLogger.Object);

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

    private GameState CreateTestGameState(string gameId = "test-game-123")
    {
        return new GameState
        {
            Timestamp = DateTime.UtcNow,
            GameInfo = new GameInfo
            {
                GameId = gameId,
                GameTime = 100.5,
                Round = 5,
                Stage = 2,
                IsPvp = true,
                SetNumber = "Set15"
            },
            ActivePlayer = new ActivePlayer
            {
                SummonerName = "TestPlayer",
                Level = 8,
                CurrentGold = 50,
                Health = 80,
                Experience = 100,
                TotalGold = 200,
                Placement = 0,
                Board = new List<BoardUnit>(),
                Bench = new List<BenchUnit>(),
                Shop = new List<ShopUnit>()
            },
            AllPlayers = new List<PlayerSummary>(),
            Augments = new List<Augment>()
        };
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        _repository.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveGameStateAsync_ShouldSaveSuccessfully()
    {
        var gameState = CreateTestGameState();

        var action = () => _repository.SaveGameStateAsync(gameState);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SaveAndGetGameStateAsync_ShouldReturnSameState()
    {
        var gameState = CreateTestGameState();
        await _repository.SaveGameStateAsync(gameState);

        var retrieved = await _repository.GetLatestGameStateAsync();

        retrieved.Should().NotBeNull();
        retrieved!.GameInfo.GameId.Should().Be(gameState.GameInfo.GameId);
        retrieved.ActivePlayer.SummonerName.Should().Be(gameState.ActivePlayer.SummonerName);
    }

    [Fact]
    public async Task GetLatestGameStateAsync_WithNoData_ShouldReturnNull()
    {
        var result = await _repository.GetLatestGameStateAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGameStateByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var result = await _repository.GetGameStateByIdAsync("invalid-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGameStatesByDateRangeAsync_WithNoData_ShouldReturnEmpty()
    {
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);

        var result = await _repository.GetGameStatesByDateRangeAsync(startDate, endDate);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGameStatesByDateRangeAsync_WithData_ShouldReturnMatchingStates()
    {
        var gameState1 = CreateTestGameState("game-1");
        gameState1.Timestamp = DateTime.UtcNow.AddHours(-2);
        var gameState2 = CreateTestGameState("game-2");
        gameState2.Timestamp = DateTime.UtcNow.AddHours(-1);

        await _repository.SaveGameStateAsync(gameState1);
        await _repository.SaveGameStateAsync(gameState2);

        var startDate = DateTime.UtcNow.AddHours(-3);
        var endDate = DateTime.UtcNow.AddHours(1);

        var result = (await _repository.GetGameStatesByDateRangeAsync(startDate, endDate)).ToList();

        result.Should().NotBeNull();
        result.Count.Should().Be(2);
    }

    [Fact]
    public async Task DeleteGameStatesOlderThanAsync_ShouldDeleteOldRecords()
    {
        var oldState = CreateTestGameState("old-game");
        oldState.Timestamp = DateTime.UtcNow.AddDays(-10);
        var newState = CreateTestGameState("new-game");
        newState.Timestamp = DateTime.UtcNow;

        await _repository.SaveGameStateAsync(oldState);
        await _repository.SaveGameStateAsync(newState);

        await _repository.DeleteGameStatesOlderThanAsync(DateTime.UtcNow.AddDays(-5));

        var result = (await _repository.GetGameStatesByDateRangeAsync(
            DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(1))).ToList();

        result.Count.Should().Be(1);
        result[0].GameInfo.GameId.Should().Be("new-game");
    }

    [Fact]
    public async Task GetLatestGameStateAsync_WithMultipleStates_ShouldReturnLatest()
    {
        var state1 = CreateTestGameState("game-1");
        state1.Timestamp = DateTime.UtcNow.AddHours(-2);
        var state2 = CreateTestGameState("game-2");
        state2.Timestamp = DateTime.UtcNow.AddHours(-1);

        await _repository.SaveGameStateAsync(state1);
        await _repository.SaveGameStateAsync(state2);

        var latest = await _repository.GetLatestGameStateAsync();

        latest.Should().NotBeNull();
        latest!.GameInfo.GameId.Should().Be("game-2");
    }

    [Fact]
    public void Constructor_WithoutLogger_ShouldStillWork()
    {
        var action = () => new SqliteGameStateRepository(_connectionString);

        action.Should().NotThrow();
    }
}
