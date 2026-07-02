using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Services;

namespace TFTAssistant.Core.Tests.Services;

public class LineupAnalysisServiceTests
{
    private readonly Mock<IBigDataService> _mockBigDataService;
    private readonly Mock<ILogger<LineupAnalysisService>> _mockLogger;
    private readonly LineupAnalysisService _lineupAnalysisService;

    public LineupAnalysisServiceTests()
    {
        _mockBigDataService = new Mock<IBigDataService>();
        _mockLogger = new Mock<ILogger<LineupAnalysisService>>();
        _lineupAnalysisService = new LineupAnalysisService(_mockBigDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        Assert.NotNull(_lineupAnalysisService);
    }

    [Fact]
    public async Task AnalyzeLineupAsync_ShouldReturnAnalysis_WhenDataAvailable()
    {
        // Arrange
        var lineupId = "lineup1";
        var setVersion = "Set 9";
        var expectedAnalysis = new Models.BigData.LineupAnalysis
        {
            LineupId = lineupId,
            WinRate = 0.65,
            PickRate = 0.15,
            AveragePlacement = 2.5
        };

        _mockBigDataService.Setup(x => x.GetLineupAnalysisAsync(lineupId, setVersion, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAnalysis);

        // Act
        var result = await _lineupAnalysisService.AnalyzeLineupAsync(lineupId, setVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAnalysis.LineupId, result.LineupId);
        Assert.Equal(expectedAnalysis.WinRate, result.WinRate);
    }

    [Fact]
    public async Task AnalyzeLineupAsync_ShouldReturnNull_WhenDataServiceReturnsNull()
    {
        // Arrange
        var lineupId = "lineup1";
        var setVersion = "Set 9";

        _mockBigDataService.Setup(x => x.GetLineupAnalysisAsync(lineupId, setVersion, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Models.BigData.LineupAnalysis)null);

        // Act
        var result = await _lineupAnalysisService.AnalyzeLineupAsync(lineupId, setVersion);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AnalyzeLineupAsync_ShouldReturnNull_WhenExceptionOccurs()
    {
        // Arrange
        var lineupId = "lineup1";
        var setVersion = "Set 9";

        _mockBigDataService.Setup(x => x.GetLineupAnalysisAsync(lineupId, setVersion, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _lineupAnalysisService.AnalyzeLineupAsync(lineupId, setVersion);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPopularLineupsAsync_ShouldReturnLineups_WhenDataAvailable()
    {
        // Arrange
        var setVersion = "Set 9";
        var expectedLineups = new List<Models.BigData.LineupData>
        {
            new Models.BigData.LineupData { Id = "lineup1", Name = "阵容1", WinRate = 0.65 },
            new Models.BigData.LineupData { Id = "lineup2", Name = "阵容2", WinRate = 0.60 }
        };

        _mockBigDataService.Setup(x => x.GetPopularLineupsAsync(setVersion, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLineups);

        // Act
        var result = await _lineupAnalysisService.GetPopularLineupsAsync(setVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedLineups.Count, result.Count);
        Assert.Equal(expectedLineups[0].Id, result[0].Id);
    }

    [Fact]
    public async Task GetPopularLineupsAsync_ShouldReturnEmptyList_WhenDataServiceReturnsNull()
    {
        // Arrange
        var setVersion = "Set 9";

        _mockBigDataService.Setup(x => x.GetPopularLineupsAsync(setVersion, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Models.BigData.LineupData>)null);

        // Act
        var result = await _lineupAnalysisService.GetPopularLineupsAsync(setVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
