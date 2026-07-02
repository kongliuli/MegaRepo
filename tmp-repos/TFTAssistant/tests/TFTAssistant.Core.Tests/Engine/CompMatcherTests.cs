using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Engine;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Tests.Engine;

public class CompMatcherTests
{
    private readonly Mock<IBigDataService> _mockBigDataService;
    private readonly Mock<ILogger<CompMatcher>> _mockLogger;
    private readonly CompMatcher _compMatcher;

    public CompMatcherTests()
    {
        _mockBigDataService = new Mock<IBigDataService>();
        _mockLogger = new Mock<ILogger<CompMatcher>>();
        _compMatcher = new CompMatcher(_mockBigDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task MatchAsync_ShouldReturnCompSuggestions_WhenDataAvailable()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>
        {
            new BoardUnit { ChampionName = "Ahri", StarLevel = 2 },
            new BoardUnit { ChampionName = "Syndra", StarLevel = 1 }
        };
        var benchUnits = new List<BenchUnit>
        {
            new BenchUnit { ChampionName = "Kai'Sa", StarLevel = 1 }
        };
        var selectedAugments = new List<Augment>
        {
            new Augment { Id = "augment1", Name = "测试强化1" }
        };

        var expectedSuggestions = new List<CompSuggestion>
        {
            new CompSuggestion { CompId = "comp1", CompName = "法师阵容", Score = 90, Reasoning = "与当前英雄匹配度高" },
            new CompSuggestion { CompId = "comp2", CompName = "星界龙阵容", Score = 75, Reasoning = "有部分英雄匹配" }
        };

        _mockBigDataService.Setup(x => x.GetCompMatchesAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSuggestions);

        // Act
        var result = await _compMatcher.MatchAsync(boardUnits, benchUnits, selectedAugments);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSuggestions.Count, result.Count);
        Assert.Equal(expectedSuggestions[0].CompId, result[0].CompId);
        Assert.Equal(expectedSuggestions[0].Score, result[0].Score);
    }

    [Fact]
    public async Task MatchAsync_ShouldReturnEmptyList_WhenNoUnits()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>();
        var benchUnits = new List<BenchUnit>();
        var selectedAugments = new List<Augment>();

        // Act
        var result = await _compMatcher.MatchAsync(boardUnits, benchUnits, selectedAugments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task MatchAsync_ShouldReturnEmptyList_WhenBigDataServiceReturnsNull()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>
        {
            new BoardUnit { ChampionName = "Ahri", StarLevel = 2 }
        };
        var benchUnits = new List<BenchUnit>();
        var selectedAugments = new List<Augment>();

        _mockBigDataService.Setup(x => x.GetCompMatchesAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<CompSuggestion>)null);

        // Act
        var result = await _compMatcher.MatchAsync(boardUnits, benchUnits, selectedAugments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task MatchAsync_ShouldReturnEmptyList_WhenExceptionOccurs()
    {
        // Arrange
        var boardUnits = new List<BoardUnit>
        {
            new BoardUnit { ChampionName = "Ahri", StarLevel = 2 }
        };
        var benchUnits = new List<BenchUnit>();
        var selectedAugments = new List<Augment>();

        _mockBigDataService.Setup(x => x.GetCompMatchesAsync(It.IsAny<List<BoardUnit>>(), It.IsAny<List<BenchUnit>>(), It.IsAny<List<Augment>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _compMatcher.MatchAsync(boardUnits, benchUnits, selectedAugments);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
