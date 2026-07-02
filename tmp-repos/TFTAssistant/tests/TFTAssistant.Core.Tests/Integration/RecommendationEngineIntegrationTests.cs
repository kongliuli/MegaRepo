using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Engine;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Tests.Integration;

public class RecommendationEngineIntegrationTests
{
    private readonly Mock<ICompMatcher> _mockCompMatcher;
    private readonly Mock<IItemAdvisor> _mockItemAdvisor;
    private readonly Mock<IEconomyAdvisor> _mockEconomyAdvisor;
    private readonly Mock<IAugmentAdvisor> _mockAugmentAdvisor;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<RecommendationEngine>> _mockLogger;
    private readonly RecommendationEngine _recommendationEngine;

    public RecommendationEngineIntegrationTests()
    {
        _mockCompMatcher = new Mock<ICompMatcher>();
        _mockItemAdvisor = new Mock<IItemAdvisor>();
        _mockEconomyAdvisor = new Mock<IEconomyAdvisor>();
        _mockAugmentAdvisor = new Mock<IAugmentAdvisor>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _mockLogger = new Mock<ILogger<RecommendationEngine>>();

        _recommendationEngine = new RecommendationEngine(
            _mockCompMatcher.Object,
            _mockItemAdvisor.Object,
            _mockEconomyAdvisor.Object,
            _mockAugmentAdvisor.Object,
            _memoryCache,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldIntegrateAllAdvisors_WhenAllServicesReturnData()
    {
        // Arrange
        var gameState = CreateMockGameState();
        var compSuggestions = new List<CompSuggestion> { new CompSuggestion { CompId = "comp1", Score = 90 } };
        var itemSuggestions = new List<ItemSuggestion> { new ItemSuggestion { ItemId = "item1", Priority = 1 } };
        var economyHint = new EconomyHint { Title = "经济提示", Urgency = HintUrgency.Low };
        var augmentRatings = new List<AugmentRating> { new AugmentRating { AugmentId = "augment1", Score = 85 } };

        _mockCompMatcher.Setup(x => x.MatchAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(compSuggestions);
        _mockItemAdvisor.Setup(x => x.AdviseAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<ItemComponent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(itemSuggestions);
        _mockEconomyAdvisor.Setup(x => x.Advise(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>()))
            .Returns(economyHint);
        _mockAugmentAdvisor.Setup(x => x.RateAsync(It.IsAny<List<Augment>>(), It.IsAny<List<Augment>>(), It.IsAny<List<BoardUnit>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(augmentRatings);

        // Act
        var result = await _recommendationEngine.GetRecommendationsAsync(gameState);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.CompSuggestions);
        Assert.Single(result.ItemSuggestions);
        Assert.NotNull(result.EconomyHint);
        Assert.Single(result.AugmentRatings);
        Assert.Equal("comp1", result.CompSuggestions[0].CompId);
        Assert.Equal("item1", result.ItemSuggestions[0].ItemId);
        Assert.Equal("经济提示", result.EconomyHint.Title);
        Assert.Equal("augment1", result.AugmentRatings[0].AugmentId);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldHandlePartialData_WhenSomeServicesFail()
    {
        // Arrange
        var gameState = CreateMockGameState();
        var compSuggestions = new List<CompSuggestion> { new CompSuggestion { CompId = "comp1", Score = 90 } };
        var economyHint = new EconomyHint { Title = "经济提示" };

        _mockCompMatcher.Setup(x => x.MatchAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(compSuggestions);
        _mockItemAdvisor.Setup(x => x.AdviseAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<ItemComponent>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Item advisor failed"));
        _mockEconomyAdvisor.Setup(x => x.Advise(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>()))
            .Returns(economyHint);
        _mockAugmentAdvisor.Setup(x => x.RateAsync(It.IsAny<List<Augment>>(), It.IsAny<List<Augment>>(), It.IsAny<List<BoardUnit>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Augment advisor failed"));

        // Act
        var result = await _recommendationEngine.GetRecommendationsAsync(gameState);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.CompSuggestions);
        Assert.Empty(result.ItemSuggestions);
        Assert.NotNull(result.EconomyHint);
        Assert.Empty(result.AugmentRatings);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldReturnEmptyResult_WhenAllServicesFail()
    {
        // Arrange
        var gameState = CreateMockGameState();

        _mockCompMatcher.Setup(x => x.MatchAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Comp matcher failed"));
        _mockItemAdvisor.Setup(x => x.AdviseAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<ItemComponent>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Item advisor failed"));
        _mockEconomyAdvisor.Setup(x => x.Advise(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>()))
            .Throws(new Exception("Economy advisor failed"));
        _mockAugmentAdvisor.Setup(x => x.RateAsync(It.IsAny<List<Augment>>(), It.IsAny<List<Augment>>(), It.IsAny<List<BoardUnit>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Augment advisor failed"));

        // Act
        var result = await _recommendationEngine.GetRecommendationsAsync(gameState);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CompSuggestions);
        Assert.Empty(result.ItemSuggestions);
        Assert.Null(result.EconomyHint);
        Assert.Empty(result.AugmentRatings);
    }

    private GameState CreateMockGameState()
    {
        return new GameState
        {
            GameInfo = new GameInfo { Stage = 3, Round = 1 },
            ActivePlayer = new ActivePlayer
            {
                Level = 5,
                TotalGold = 40,
                Health = 80,
                WinStreak = 2,
                LoseStreak = 0,
                Augments = new List<Augment> { new Augment { Id = "augment1" } },
                Items = new List<string> { "Tear", "Sword" }
            },
            BoardUnits = new List<BoardUnit>
            {
                new BoardUnit { ChampionName = "Ahri", StarLevel = 2, Items = new List<string> { "Tear" } }
            },
            BenchUnits = new List<BenchUnit>
            {
                new BenchUnit { ChampionName = "Kai'Sa", StarLevel = 1, Items = new List<string> { "Sword" } }
            },
            AvailableAugments = new List<Augment>
            {
                new Augment { Id = "augment2" },
                new Augment { Id = "augment3" }
            }
        };
    }
}
