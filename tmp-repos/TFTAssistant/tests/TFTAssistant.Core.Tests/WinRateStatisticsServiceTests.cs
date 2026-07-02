using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Analytics;
using TFTAssistant.Core.Services;

namespace TFTAssistant.Core.Tests;

public class WinRateStatisticsServiceTests
{
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<ILogger<WinRateStatisticsService>> _mockLogger;
    private readonly WinRateStatisticsService _service;

    public WinRateStatisticsServiceTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<WinRateStatisticsService>>();
        _service = new WinRateStatisticsService(
            _mockMatchRepository.Object,
            _mockCache.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOverallStatisticsAsync_WithCachedData_ShouldReturnCachedValue()
    {
        var playerPuuid = "test-puuid-123";
        var cachedStats = new MatchStats
        {
            Id = Guid.NewGuid().ToString(),
            PlayerPuuid = playerPuuid,
            TotalMatches = 10,
            Wins = 3
        };

        object outValue = cachedStats;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(true);

        var result = await _service.GetOverallStatisticsAsync(playerPuuid);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedStats);
        _mockMatchRepository.Verify(x => x.GetMatchStatsAsync(
            It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOverallStatisticsAsync_WithoutCachedData_ShouldFetchFromRepository()
    {
        var playerPuuid = "test-puuid-123";
        var statsFromRepo = new MatchStats
        {
            Id = Guid.NewGuid().ToString(),
            PlayerPuuid = playerPuuid,
            TotalMatches = 10,
            Wins = 3
        };

        object outValue = null!;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(false);

        _mockMatchRepository
            .Setup(x => x.GetMatchStatsAsync(playerPuuid, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statsFromRepo);

        _mockCache
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        var result = await _service.GetOverallStatisticsAsync(playerPuuid);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(statsFromRepo);
        _mockMatchRepository.Verify(x => x.GetMatchStatsAsync(
            playerPuuid, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOverallStatisticsAsync_WithNullStatsFromRepo_ShouldNotCache()
    {
        var playerPuuid = "test-puuid-123";
        object outValue = null!;

        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(false);

        _mockMatchRepository
            .Setup(x => x.GetMatchStatsAsync(playerPuuid, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MatchStats?)null);

        var result = await _service.GetOverallStatisticsAsync(playerPuuid);

        result.Should().BeNull();
        _mockCache.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task GetCompStatisticsAsync_WithCachedData_ShouldReturnCachedValue()
    {
        var playerPuuid = "test-puuid-123";
        var cachedStats = new List<CompStats>
        {
            new() { CompId = "comp1", TotalGames = 5 }
        };

        object outValue = cachedStats;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(true);

        var result = await _service.GetCompStatisticsAsync(playerPuuid);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedStats);
        _mockMatchRepository.Verify(x => x.GetCompStatsAsync(
            It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCompStatisticsAsync_WithoutCachedData_ShouldFetchFromRepository()
    {
        var playerPuuid = "test-puuid-123";
        var statsFromRepo = new List<CompStats>
        {
            new() { CompId = "comp1", TotalGames = 5 }
        };

        object outValue = null!;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(false);

        _mockMatchRepository
            .Setup(x => x.GetCompStatsAsync(playerPuuid, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statsFromRepo);

        _mockCache
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        var result = await _service.GetCompStatisticsAsync(playerPuuid);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(statsFromRepo);
        _mockMatchRepository.Verify(x => x.GetCompStatsAsync(
            playerPuuid, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetVersionStatisticsAsync_WithCachedData_ShouldReturnCachedValue()
    {
        var playerPuuid = "test-puuid-123";
        var cachedStats = new List<VersionStats>
        {
            new() { SetVersion = "15.1", TotalMatches = 10 }
        };

        object outValue = cachedStats;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(true);

        var result = await _service.GetVersionStatisticsAsync(playerPuuid);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(cachedStats);
        _mockMatchRepository.Verify(x => x.GetVersionStatsAsync(
            It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetVersionStatisticsAsync_WithoutCachedData_ShouldFetchFromRepository()
    {
        var playerPuuid = "test-puuid-123";
        var statsFromRepo = new List<VersionStats>
        {
            new() { SetVersion = "15.1", TotalMatches = 10 }
        };

        object outValue = null!;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(false);

        _mockMatchRepository
            .Setup(x => x.GetVersionStatsAsync(playerPuuid, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statsFromRepo);

        _mockCache
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        var result = await _service.GetVersionStatisticsAsync(playerPuuid);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(statsFromRepo);
        _mockMatchRepository.Verify(x => x.GetVersionStatsAsync(
            playerPuuid, null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ClearCacheAsync_ShouldRemoveCachedKeys()
    {
        var playerPuuid = "test-puuid-123";

        await _service.ClearCacheAsync(playerPuuid);

        _mockCache.Verify(x => x.Remove(It.Is<string>(k => k.Contains(playerPuuid))), Times.Exactly(3));
    }

    [Fact]
    public async Task GetOverallStatisticsAsync_WithDateRange_ShouldPassDatesToRepository()
    {
        var playerPuuid = "test-puuid-123";
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var statsFromRepo = new MatchStats
        {
            Id = Guid.NewGuid().ToString(),
            PlayerPuuid = playerPuuid,
            TotalMatches = 5
        };

        object outValue = null!;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out outValue))
            .Returns(false);

        _mockMatchRepository
            .Setup(x => x.GetMatchStatsAsync(playerPuuid, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statsFromRepo);

        _mockCache
            .Setup(x => x.CreateEntry(It.IsAny<object>()))
            .Returns(Mock.Of<ICacheEntry>());

        await _service.GetOverallStatisticsAsync(playerPuuid, startDate, endDate);

        _mockMatchRepository.Verify(x => x.GetMatchStatsAsync(
            playerPuuid, startDate, endDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithoutLogger_ShouldStillWork()
    {
        var action = () => new WinRateStatisticsService(
            _mockMatchRepository.Object,
            _mockCache.Object);

        action.Should().NotThrow();
    }
}
