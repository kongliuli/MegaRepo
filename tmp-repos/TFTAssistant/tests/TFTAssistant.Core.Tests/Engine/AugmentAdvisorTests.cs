using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Engine;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Tests.Engine;

public class AugmentAdvisorTests
{
    private readonly Mock<IBigDataService> _mockBigDataService;
    private readonly Mock<ILogger<AugmentAdvisor>> _mockLogger;
    private readonly AugmentAdvisor _augmentAdvisor;

    public AugmentAdvisorTests()
    {
        _mockBigDataService = new Mock<IBigDataService>();
        _mockLogger = new Mock<ILogger<AugmentAdvisor>>();
        _augmentAdvisor = new AugmentAdvisor(_mockBigDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RateAsync_ShouldReturnAugmentRatings_WhenDataAvailable()
    {
        // Arrange
        var availableAugments = new List<Augment>
        {
            new Augment { Id = "augment1", Name = "测试强化1" },
            new Augment { Id = "augment2", Name = "测试强化2" }
        };
        var selectedAugments = new List<Augment>
        {
            new Augment { Id = "augment3", Name = "已选强化1" }
        };
        var boardUnits = new List<BoardUnit>
        {
            new BoardUnit { ChampionName = "Ahri", StarLevel = 2 }
        };

        var expectedRatings = new List<AugmentRating>
        {
            new AugmentRating { AugmentId = "augment1", Score = 85, Reasoning = "适合当前阵容" },
            new AugmentRating { AugmentId = "augment2", Score = 60, Reasoning = "一般适合" }
        };

        _mockBigDataService.Setup(x => x.GetAugmentRatingsAsync(It.IsAny<List<Augment>>(), It.IsAny<List<Augment>>(), It.IsAny<List<BoardUnit>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRatings);

        // Act
        var result = await _augmentAdvisor.RateAsync(availableAugments, selectedAugments, boardUnits);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRatings.Count, result.Count);
        Assert.Equal(expectedRatings[0].AugmentId, result[0].AugmentId);
        Assert.Equal(expectedRatings[0].Score, result[0].Score);
    }

    [Fact]
    public async Task RateAsync_ShouldReturnEmptyList_WhenNoAvailableAugments()
    {
        // Arrange
        var availableAugments = new List<Augment>();
        var selectedAugments = new List<Augment>();
        var boardUnits = new List<BoardUnit>();

        // Act
        var result = await _augmentAdvisor.RateAsync(availableAugments, selectedAugments, boardUnits);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task RateAsync_ShouldReturnEmptyList_WhenBigDataServiceReturnsNull()
    {
        // Arrange
        var availableAugments = new List<Augment>
        {
            new Augment { Id = "augment1", Name = "测试强化1" }
        };
        var selectedAugments = new List<Augment>();
        var boardUnits = new List<BoardUnit>();

        _mockBigDataService.Setup(x => x.GetAugmentRatingsAsync(It.IsAny<List<Augment>>(), It.IsAny<List<Augment>>(), It.IsAny<List<BoardUnit>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<AugmentRating>)null);

        // Act
        var result = await _augmentAdvisor.RateAsync(availableAugments, selectedAugments, boardUnits);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task RateAsync_ShouldReturnEmptyList_WhenExceptionOccurs()
    {
        // Arrange
        var availableAugments = new List<Augment>
        {
            new Augment { Id = "augment1", Name = "测试强化1" }
        };
        var selectedAugments = new List<Augment>();
        var boardUnits = new List<BoardUnit>();

        _mockBigDataService.Setup(x => x.GetAugmentRatingsAsync(It.IsAny<List<Augment>>(), It.IsAny<List<Augment>>(), It.IsAny<List<BoardUnit>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _augmentAdvisor.RateAsync(availableAugments, selectedAugments, boardUnits);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
