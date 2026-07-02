using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Services;

namespace TFTAssistant.Core.Tests.Services;

public class EquipmentManagerTests
{
    private readonly Mock<IEquipmentDataService> _mockEquipmentDataService;
    private readonly Mock<ILogger<EquipmentManager>> _mockLogger;
    private readonly EquipmentManager _equipmentManager;

    public EquipmentManagerTests()
    {
        _mockEquipmentDataService = new Mock<IEquipmentDataService>();
        _mockLogger = new Mock<ILogger<EquipmentManager>>();
        _equipmentManager = new EquipmentManager(_mockEquipmentDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        Assert.NotNull(_equipmentManager);
    }

    [Fact]
    public async Task GetRecommendedItemsAsync_ShouldReturnRecommendations_WhenDataAvailable()
    {
        // Arrange
        var championId = "champion1";
        var setVersion = "Set 9";
        var expectedRecommendations = new List<Models.Recommendation.ItemSuggestion>
        {
            new Models.Recommendation.ItemSuggestion { ItemId = "item1", Priority = 1 },
            new Models.Recommendation.ItemSuggestion { ItemId = "item2", Priority = 2 }
        };

        _mockEquipmentDataService.Setup(x => x.GetRecommendedItemsAsync(championId, setVersion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecommendations);

        // Act
        var result = await _equipmentManager.GetRecommendedItemsAsync(championId, setVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRecommendations.Count, result.Count);
        Assert.Equal(expectedRecommendations[0].ItemId, result[0].ItemId);
    }

    [Fact]
    public async Task GetRecommendedItemsAsync_ShouldReturnEmptyList_WhenDataServiceReturnsNull()
    {
        // Arrange
        var championId = "champion1";
        var setVersion = "Set 9";

        _mockEquipmentDataService.Setup(x => x.GetRecommendedItemsAsync(championId, setVersion, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Models.Recommendation.ItemSuggestion>)null);

        // Act
        var result = await _equipmentManager.GetRecommendedItemsAsync(championId, setVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendedItemsAsync_ShouldReturnEmptyList_WhenExceptionOccurs()
    {
        // Arrange
        var championId = "champion1";
        var setVersion = "Set 9";

        _mockEquipmentDataService.Setup(x => x.GetRecommendedItemsAsync(championId, setVersion, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _equipmentManager.GetRecommendedItemsAsync(championId, setVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
