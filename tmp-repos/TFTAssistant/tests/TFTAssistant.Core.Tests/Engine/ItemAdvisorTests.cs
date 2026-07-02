using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Engine;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Tests.Engine;

public class ItemAdvisorTests
{
    private readonly Mock<IBigDataService> _mockBigDataService;
    private readonly Mock<ILogger<ItemAdvisor>> _mockLogger;
    private readonly ItemAdvisor _itemAdvisor;

    public ItemAdvisorTests()
    {
        _mockBigDataService = new Mock<IBigDataService>();
        _mockLogger = new Mock<ILogger<ItemAdvisor>>();
        _itemAdvisor = new ItemAdvisor(_mockBigDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AdviseAsync_ShouldReturnItemSuggestions_WhenDataAvailable()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>
        {
            new BoardUnit { ChampionName = "Ahri", StarLevel = 2 },
            new BoardUnit { ChampionName = "Kai'Sa", StarLevel = 1 }
        };
        var availableComponents = new List<ItemComponent>
        {
            new ItemComponent { Id = "Tear" },
            new ItemComponent { Id = "Sword" }
        };

        var expectedSuggestions = new List<ItemSuggestion>
        {
            new ItemSuggestion { ItemId = "BlueBuff", Priority = 1, TargetChampion = "Ahri", Reasoning = "适合法师英雄" },
            new ItemSuggestion { ItemId = "InfinityEdge", Priority = 2, TargetChampion = "Kai'Sa", Reasoning = "适合射手英雄" }
        };

        _mockBigDataService.Setup(x => x.GetItemRecommendationsAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<ItemComponent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSuggestions);

        // Act
        var result = await _itemAdvisor.AdviseAsync(boardUnits, availableComponents);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSuggestions.Count, result.Count);
        Assert.Equal(expectedSuggestions[0].ItemId, result[0].ItemId);
        Assert.Equal(expectedSuggestions[0].Priority, result[0].Priority);
    }

    [Fact]
    public async Task AdviseAsync_ShouldReturnEmptyList_WhenNoUnits()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>();
        var availableComponents = new List<ItemComponent>();

        // Act
        var result = await _itemAdvisor.AdviseAsync(boardUnits, availableComponents);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AdviseAsync_ShouldReturnEmptyList_WhenNoComponents()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>
        {
            new BoardUnit { ChampionName = "Ahri", StarLevel = 2 }
        };
        var availableComponents = new List<ItemComponent>();

        // Act
        var result = await _itemAdvisor.AdviseAsync(boardUnits, availableComponents);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AdviseAsync_ShouldReturnEmptyList_WhenBigDataServiceReturnsNull()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>
        {
            new BoardUnit { ChampionName = "Ahri", StarLevel = 2 }
        };
        var availableComponents = new List<ItemComponent>
        {
            new ItemComponent { Id = "Tear" }
        };

        _mockBigDataService.Setup(x => x.GetItemRecommendationsAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<ItemComponent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<ItemSuggestion>)null);

        // Act
        var result = await _itemAdvisor.AdviseAsync(boardUnits, availableComponents);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task AdviseAsync_ShouldReturnEmptyList_WhenExceptionOccurs()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>
        {
            new BoardUnit { ChampionName = "Ahri", StarLevel = 2 }
        };
        var availableComponents = new List<ItemComponent>
        {
            new ItemComponent { Id = "Tear" }
        };

        _mockBigDataService.Setup(x => x.GetItemRecommendationsAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<ItemComponent>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _itemAdvisor.AdviseAsync(boardUnits, availableComponents);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
