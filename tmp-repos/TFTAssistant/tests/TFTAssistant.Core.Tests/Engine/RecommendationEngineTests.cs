using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Engine;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Tests.Engine;

public class RecommendationEngineTests
{
    private readonly Mock<ICompMatcher> _mockCompMatcher;
    private readonly Mock<IItemAdvisor> _mockItemAdvisor;
    private readonly Mock<IEconomyAdvisor> _mockEconomyAdvisor;
    private readonly Mock<IAugmentAdvisor> _mockAugmentAdvisor;
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<RecommendationEngine>> _mockLogger;
    private readonly RecommendationEngine _recommendationEngine;

    public RecommendationEngineTests()
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
    public async Task GetRecommendationsAsync_ShouldReturnCachedResult_WhenCacheExists()
    {
        // Arrange
        var gameState = CreateMockGameState();
        var cachedResult = new RecommendationSet
        {
            CompSuggestions = new List<CompSuggestion>(),
            ItemSuggestions = new List<ItemSuggestion>(),
            EconomyHint = new EconomyHint(),
            AugmentRatings = new List<AugmentRating>()
        };

        var cacheKey = GenerateCacheKey(gameState);
        _memoryCache.Set(cacheKey, cachedResult);

        // Act
        var result = await _recommendationEngine.GetRecommendationsAsync(gameState);

        // Assert
        Assert.Same(cachedResult, result);
        _mockCompMatcher.Verify(x => x.MatchAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldReturnAllRecommendations_WhenAllTasksComplete()
    {
        // Arrange
        var gameState = CreateMockGameState();
        var compSuggestions = new List<CompSuggestion> { new CompSuggestion { CompId = "comp1", Score = 90 } };
        var itemSuggestions = new List<ItemSuggestion> { new ItemSuggestion { ItemId = "item1", Priority = 1 } };
        var economyHint = new EconomyHint { Title = "Test Hint" };
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
        Assert.Equal(compSuggestions, result.CompSuggestions);
        Assert.Equal(itemSuggestions, result.ItemSuggestions);
        Assert.Equal(economyHint, result.EconomyHint);
        Assert.Equal(augmentRatings, result.AugmentRatings);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldReturnPartialResults_WhenSomeTasksTimeout()
    {
        // Arrange
        var gameState = CreateMockGameState();
        var compSuggestions = new List<CompSuggestion> { new CompSuggestion { CompId = "comp1", Score = 90 } };
        var economyHint = new EconomyHint { Title = "Test Hint" };

        _mockCompMatcher.Setup(x => x.MatchAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(compSuggestions);
        _mockItemAdvisor.Setup(x => x.AdviseAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<ItemComponent>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.Delay(1000).ContinueWith(_ => new List<ItemSuggestion>()));
        _mockEconomyAdvisor.Setup(x => x.Advise(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>()))
            .Returns(economyHint);
        _mockAugmentAdvisor.Setup(x => x.RateAsync(It.IsAny<List<Augment>>(), It.IsAny<List<Augment>>(), It.IsAny<List<BoardUnit>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.Delay(1000).ContinueWith(_ => new List<AugmentRating>()));

        // Act
        var result = await _recommendationEngine.GetRecommendationsAsync(gameState);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(compSuggestions, result.CompSuggestions);
        Assert.Empty(result.ItemSuggestions);
        Assert.Equal(economyHint, result.EconomyHint);
        Assert.Empty(result.AugmentRatings);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldReturnEmptyResult_WhenOperationCanceled()
    {
        // Arrange
        var gameState = CreateMockGameState();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _recommendationEngine.GetRecommendationsAsync(gameState, cts.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.CompSuggestions);
        Assert.Empty(result.ItemSuggestions);
        Assert.Null(result.EconomyHint);
        Assert.Empty(result.AugmentRatings);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldReturnEmptyResult_WhenExceptionOccurs()
    {
        // Arrange
        var gameState = CreateMockGameState();

        _mockCompMatcher.Setup(x => x.MatchAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

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

    private string GenerateCacheKey(GameState state)
    {
        var keyBuilder = new System.Text.StringBuilder();
        keyBuilder.Append($"stage:{state.GameInfo.Stage}");
        keyBuilder.Append($"_gold:{state.ActivePlayer.TotalGold}");
        keyBuilder.Append($"_level:{state.ActivePlayer.Level}");
        keyBuilder.Append($"_health:{state.ActivePlayer.Health}");
        keyBuilder.Append($"_winStreak:{state.ActivePlayer.WinStreak}");
        keyBuilder.Append($"_loseStreak:{state.ActivePlayer.LoseStreak}");

        foreach (var unit in state.BoardUnits.OrderBy(u => u.ChampionName))
        {
            keyBuilder.Append($"_board:{unit.ChampionName}:{unit.StarLevel}");
        }

        foreach (var unit in state.BenchUnits.OrderBy(u => u.ChampionName))
        {
            keyBuilder.Append($"_bench:{unit.ChampionName}:{unit.StarLevel}");
        }

        foreach (var augment in state.ActivePlayer.Augments.OrderBy(a => a.Id))
        {
            keyBuilder.Append($"_augment:{augment.Id}");
        }

        if (state.AvailableAugments != null)
        {
            foreach (var augment in state.AvailableAugments.OrderBy(a => a.Id))
            {
                keyBuilder.Append($"_availableAugment:{augment.Id}");
            }
        }

        return keyBuilder.ToString();
    }
}
