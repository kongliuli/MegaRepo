using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Tests;

public class DataDragonProviderMultiVersionTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<DataDragonProvider>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly string _tempDataDir;
    private readonly DataDragonProvider _provider;

    public DataDragonProviderMultiVersionTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<DataDragonProvider>>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _tempDataDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDataDir);
        _provider = new DataDragonProvider(_httpClient, _mockLogger.Object, _tempDataDir);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        if (Directory.Exists(_tempDataDir))
        {
            Directory.Delete(_tempDataDir, true);
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        _provider.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAvailableVersionsAsync_WithHttpSuccess_ShouldReturnVersions()
    {
        var versions = new List<string> { "15.1.1", "14.23.1", "14.22.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var result = await _provider.GetAvailableVersionsAsync();

        result.Should().NotBeNull();
        result.Count.Should().Be(3);
        result[0].Should().Be("15.1.1");
    }

    [Fact]
    public async Task GetAvailableVersionsAsync_WithHttpError_ShouldReturnEmptyList()
    {
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _provider.GetAvailableVersionsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLatestVersionAsync_WithAvailableVersions_ShouldReturnFirstVersion()
    {
        var versions = new List<string> { "15.1.1", "14.23.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var result = await _provider.GetLatestVersionAsync();

        result.Should().Be("15.1.1");
    }

    [Fact]
    public async Task GetLatestVersionAsync_WithoutAvailableVersions_ShouldReturnDefault()
    {
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json", "[]");

        var result = await _provider.GetLatestVersionAsync();

        result.Should().Be("15.1.1");
    }

    [Fact]
    public async Task GetChampionsAsync_WithValidVersion_ShouldReturnChampions()
    {
        var versions = new List<string> { "15.1.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var championData = new
        {
            data = new Dictionary<string, object>
            {
                ["TFT8_Ahri"] = new { name = "Ahri", cost = 3 }
            }
        };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/cdn/15.1.1/data/en_US/tft-champion.json",
            JsonSerializer.Serialize(championData));

        var result = await _provider.GetChampionsAsync("Set15", "15.1.1");

        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetItemsAsync_WithValidVersion_ShouldReturnItems()
    {
        var versions = new List<string> { "15.1.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var itemData = new
        {
            data = new Dictionary<string, object>
            {
                ["TFT_Item_Bloodthirster"] = new { name = "Bloodthirster", description = "Lifesteal" }
            }
        };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/cdn/15.1.1/data/en_US/tft-item.json",
            JsonSerializer.Serialize(itemData));

        var result = await _provider.GetItemsAsync("Set15", "15.1.1");

        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTraitsAsync_WithValidVersion_ShouldReturnTraits()
    {
        var versions = new List<string> { "15.1.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var traitData = new
        {
            data = new Dictionary<string, object>
            {
                ["TFT8_Trait_Arcanist"] = new { name = "Arcanist" }
            }
        };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/cdn/15.1.1/data/en_US/tft-trait.json",
            JsonSerializer.Serialize(traitData));

        var result = await _provider.GetTraitsAsync("Set15", "15.1.1");

        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAugmentsAsync_ShouldReturnEmptyList()
    {
        var versions = new List<string> { "15.1.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var result = await _provider.GetAugmentsAsync("Set15", "15.1.1");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMetaCompsAsync_ShouldReturnEmptyList()
    {
        var versions = new List<string> { "15.1.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var result = await _provider.GetMetaCompsAsync("Set15", "15.1.1");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearVersionCacheAsync_WithSpecificVersion_ShouldClearVersionCache()
    {
        var versions = new List<string> { "15.1.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var versionDir = Path.Combine(_tempDataDir, "versions", "15.1.1");
        Directory.CreateDirectory(versionDir);

        var action = () => _provider.ClearVersionCacheAsync("15.1.1");

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearVersionCacheAsync_WithoutVersion_ShouldClearAllCaches()
    {
        var versions = new List<string> { "15.1.1" };
        SetupHttpResponse("https://ddragon.leagueoflegends.com/api/versions.json",
            JsonSerializer.Serialize(versions));

        var versionsDir = Path.Combine(_tempDataDir, "versions");
        Directory.CreateDirectory(versionsDir);

        var action = () => _provider.ClearVersionCacheAsync();

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetCachedVersionsAsync_WithNoCachedVersions_ShouldReturnEmptyList()
    {
        var result = await _provider.GetCachedVersionsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    private void SetupHttpResponse(string url, string content)
    {
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
    }
}
