using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Services;

namespace TFTAssistant.Core.Tests.Services;

public class MetaAnalysisServiceTests
{
    private readonly Mock<IBigDataService> _mockBigDataService;
    private readonly Mock<ILogger<MetaAnalysisService>> _mockLogger;
    private readonly MetaAnalysisService _metaAnalysisService;

    public MetaAnalysisServiceTests()
    {
        _mockBigDataService = new Mock<IBigDataService>();
        _mockLogger = new Mock<ILogger<MetaAnalysisService>>();
        _metaAnalysisService = new MetaAnalysisService(_mockBigDataService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Assert
        Assert.NotNull(_metaAnalysisService);
    }

    [Fact]
    public async Task GetMetaChampionsAsync_ShouldReturnChampions_WhenDataAvailable()
    {
        // Arrange
        var setVersion = "Set 9";
        var tier = "diamond";
        var expectedChampions = new List<Models.BigData.ChampionData>
        {
            new Models.BigData.ChampionData { Id = "champion1", Name = "英雄1", WinRate = 0.65 },
            new Models.BigData.ChampionData { Id = "champion2", Name = "英雄2", WinRate = 0.60 }
        };

        _mockBigDataService.Setup(x => x.GetMetaChampionsAsync(setVersion, tier, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedChampions);

        // Act
        var result = await _metaAnalysisService.GetMetaChampionsAsync(setVersion, tier);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedChampions.Count, result.Count);
        Assert.Equal(expectedChampions[0].Id, result[0].Id);
    }

    [Fact]
    public async Task GetMetaChampionsAsync_ShouldReturnEmptyList_WhenDataServiceReturnsNull()
    {
        // Arrange
        var setVersion = "Set 9";
        var tier = "diamond";

        _mockBigDataService.Setup(x => x.GetMetaChampionsAsync(setVersion, tier, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Models.BigData.ChampionData>)null);

        // Act
        var result = await _metaAnalysisService.GetMetaChampionsAsync(setVersion, tier);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMetaChampionsAsync_ShouldReturnEmptyList_WhenExceptionOccurs()
    {
        // Arrange
        var setVersion = "Set 9";
        var tier = "diamond";

        _mockBigDataService.Setup(x => x.GetMetaChampionsAsync(setVersion, tier, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _metaAnalysisService.GetMetaChampionsAsync(setVersion, tier);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetMetaLineupsAsync_ShouldReturnLineups_WhenDataAvailable()
    {
        // Arrange
        var setVersion = "Set 9";
        var tier = "platinum";
        var expectedLineups = new List<Models.BigData.LineupData>
        {
            new Models.BigData.LineupData { Id = "lineup1", Name = "阵容1", WinRate = 0.65 },
            new Models.BigData.LineupData { Id = "lineup2", Name = "阵容2", WinRate = 0.60 }
        };

        _mockBigDataService.Setup(x => x.GetMetaLineupsAsync(setVersion, tier, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLineups);

        // Act
        var result = await _metaAnalysisService.GetMetaLineupsAsync(setVersion, tier);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedLineups.Count, result.Count);
        Assert.Equal(expectedLineups[0].Id, result[0].Id);
    }

    [Fact]
    public async Task GetMetaLineupsAsync_ShouldReturnEmptyList_WhenDataServiceReturnsNull()
    {
        // Arrange
        var setVersion = "Set 9";
        var tier = "platinum";

        _mockBigDataService.Setup(x => x.GetMetaLineupsAsync(setVersion, tier, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Models.BigData.LineupData>)null);

        // Act
        var result = await _metaAnalysisService.GetMetaLineupsAsync(setVersion, tier);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
