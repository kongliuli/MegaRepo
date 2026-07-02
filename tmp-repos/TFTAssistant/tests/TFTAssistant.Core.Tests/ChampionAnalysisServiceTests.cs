using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;
using TFTAssistant.Core.Services;

namespace TFTAssistant.Core.Tests;

public class ChampionAnalysisServiceTests
{
    private readonly Mock<IBigDataService> _bigDataServiceMock;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<ChampionAnalysisService>> _loggerMock;
    private readonly ChampionAnalysisService _championAnalysisService;

    public ChampionAnalysisServiceTests()
    {
        _bigDataServiceMock = new Mock<IBigDataService>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<ChampionAnalysisService>>();
        _championAnalysisService = new ChampionAnalysisService(
            _bigDataServiceMock.Object,
            _memoryCache,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetChampionAnalysisAsync_ShouldReturnAnalysis()
    {
        // Arrange
        var setVersion = "Set 9";
        var championId = "champion_1";
        var expectedAnalysis = new ChampionAnalysis
        {
            ChampionId = championId,
            ChampionName = "英雄 1",
            SetVersion = setVersion,
            OverallWinRate = 0.55,
            OverallPickRate = 0.15,
            AveragePlacement = 3.0,
            MatchCount = 10000,
            LastUpdated = DateTime.Now,
            IsHistoricalData = true
        };

        _bigDataServiceMock
            .Setup(x => x.GetChampionAnalysisAsync(setVersion, championId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalysis);

        // Act
        var result = await _championAnalysisService.GetChampionAnalysisAsync(setVersion, championId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAnalysis.ChampionId, result.ChampionId);
        Assert.Equal(expectedAnalysis.OverallWinRate, result.OverallWinRate);
        Assert.True(result.IsHistoricalData);
        _bigDataServiceMock.Verify(x => x.GetChampionAnalysisAsync(setVersion, championId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChampionsByTierAsync_ShouldReturnChampions()
    {
        // Arrange
        var setVersion = "Set 9";
        var tier = "diamond";
        var expectedChampions = new List<ChampionData>
        {
            new ChampionData
            {
                Id = "champion_1",
                Name = "英雄 1",
                SetVersion = setVersion,
                WinRate = 0.60,
                PickRate = 0.20,
                IsHistoricalData = true
            }
        };

        _bigDataServiceMock
            .Setup(x => x.GetChampionsByTierAsync(setVersion, tier, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedChampions);

        // Act
        var result = await _championAnalysisService.GetChampionsByTierAsync(setVersion, tier);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.First().IsHistoricalData);
        _bigDataServiceMock.Verify(x => x.GetChampionsByTierAsync(setVersion, tier, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChampionTrendsAsync_ShouldReturnTrends()
    {
        // Arrange
        var setVersion = "Set 9";
        var championId = "champion_1";
        var days = 7;
        var expectedTrends = new List<ChampionTrend>
        {
            new ChampionTrend
            {
                Date = DateTime.Now.AddDays(-1),
                WinRate = 0.55,
                PickRate = 0.15,
                MatchCount = 1000
            }
        };

        _bigDataServiceMock
            .Setup(x => x.GetChampionTrendsAsync(setVersion, championId, days, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTrends);

        // Act
        var result = await _championAnalysisService.GetChampionTrendsAsync(setVersion, championId, days);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _bigDataServiceMock.Verify(x => x.GetChampionTrendsAsync(setVersion, championId, days, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChampionEquipmentRecommendationsAsync_ShouldReturnRecommendations()
    {
        // Arrange
        var setVersion = "Set 9";
        var championId = "champion_1";
        var expectedRecommendations = new List<EquipmentRecommendation>
        {
            new EquipmentRecommendation
            {
                EquipmentId = "equipment_1",
                EquipmentName = "狂徒铠甲",
                WinRate = 0.65,
                PickRate = 0.30,
                MatchCount = 2000,
                Priority = 1
            }
        };

        _bigDataServiceMock
            .Setup(x => x.GetChampionEquipmentRecommendationsAsync(setVersion, championId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecommendations);

        // Act
        var result = await _championAnalysisService.GetChampionEquipmentRecommendationsAsync(setVersion, championId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _bigDataServiceMock.Verify(x => x.GetChampionEquipmentRecommendationsAsync(setVersion, championId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetChampionLineupPerformanceAsync_ShouldReturnPerformance()
    {
        // Arrange
        var setVersion = "Set 9";
        var championId = "champion_1";
        var expectedPerformance = new List<ChampionLineupPerformance>
        {
            new ChampionLineupPerformance
            {
                LineupId = "lineup_1",
                LineupName = "阵容 1",
                WinRate = 0.60,
                PickRate = 0.15,
                MatchCount = 1500,
                AveragePlacement = 2.5,
                SynergyScore = 85
            }
        };

        _bigDataServiceMock
            .Setup(x => x.GetChampionLineupPerformanceAsync(setVersion, championId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerformance);

        // Act
        var result = await _championAnalysisService.GetChampionLineupPerformanceAsync(setVersion, championId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _bigDataServiceMock.Verify(x => x.GetChampionLineupPerformanceAsync(setVersion, championId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ClearCacheAsync_ShouldExecuteWithoutError()
    {
        // Act & Assert
        await _championAnalysisService.ClearCacheAsync();
        // Just verify it doesn't throw an exception
    }
}
