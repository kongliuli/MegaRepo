using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Tests;

public class CompositeGameDataProviderTests : IDisposable
{
    private readonly Mock<ILogger<CompositeGameDataProvider>> _mockLogger;
    private readonly Mock<LiveClientDataProvider> _mockLiveClientProvider;
    private readonly Mock<OverwolfEventAdapter> _mockOverwolfAdapter;
    private readonly CompositeGameDataProvider _provider;

    public CompositeGameDataProviderTests()
    {
        _mockLogger = new Mock<ILogger<CompositeGameDataProvider>>();
        _mockLiveClientProvider = new Mock<LiveClientDataProvider>(Mock.Of<ILogger<LiveClientDataProvider>>());
        _mockOverwolfAdapter = new Mock<OverwolfEventAdapter>(Mock.Of<ILogger<OverwolfEventAdapter>>());
        _provider = new CompositeGameDataProvider(
            _mockLiveClientProvider.Object,
            _mockOverwolfAdapter.Object,
            _mockLogger.Object);
    }

    public void Dispose()
    {
        _provider.StopAsync().Wait();
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        _provider.IsConnected.Should().BeFalse();
        _provider.Type.Should().Be(DataSourceType.LiveClientApi);
    }

    [Fact]
    public async Task StartAsync_ShouldCallStartOnBothProviders()
    {
        await _provider.StartAsync(CancellationToken.None);

        _mockLiveClientProvider.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockOverwolfAdapter.Verify(x => x.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldCallStopOnBothProviders()
    {
        await _provider.StartAsync(CancellationToken.None);
        await _provider.StopAsync();

        _mockLiveClientProvider.Verify(x => x.StopAsync(), Times.Once);
        _mockOverwolfAdapter.Verify(x => x.StopAsync(), Times.Once);
    }

    [Fact]
    public void SelectDataSource_ShouldSwitchToSpecifiedType()
    {
        _provider.SelectDataSource(DataSourceType.OverwolfEvents);

        _provider.Type.Should().Be(DataSourceType.OverwolfEvents);
    }

    [Fact]
    public void ResetToAutomatic_ShouldResetManualOverride()
    {
        _provider.SelectDataSource(DataSourceType.OverwolfEvents);
        _provider.ResetToAutomatic();

        _provider.Type.Should().Be(DataSourceType.LiveClientApi);
    }

    [Fact]
    public async Task GetFullStateAsync_ShouldReturnNullInitially()
    {
        var state = await _provider.GetFullStateAsync();

        state.Should().BeNull();
    }

    [Fact]
    public void StateChanged_ActiveProviderChanged_ShouldNotTriggerEvent()
    {
        var eventTriggered = false;
        _provider.StateChanged += (sender, args) => eventTriggered = true;

        var diff = new GameStateDiff();
        _mockLiveClientProvider.Raise(x => x.StateChanged += null, _mockLiveClientProvider.Object, diff);

        eventTriggered.Should().BeFalse();
    }

    [Fact]
    public void SelectDataSource_WithInvalidType_ShouldNotSwitch()
    {
        var invalidType = (DataSourceType)999;
        var action = () => _provider.SelectDataSource(invalidType);

        action.Should().NotThrow();
    }

    [Fact]
    public async Task StartStop_WithCancellationToken_ShouldHandleGracefully()
    {
        var cts = new CancellationTokenSource();
        await _provider.StartAsync(cts.Token);
        cts.Cancel();

        var action = () => _provider.StopAsync();

        action.Should().NotThrow();
    }

    [Fact]
    public void MultipleSelectDataSource_ShouldHandleMultipleCalls()
    {
        _provider.SelectDataSource(DataSourceType.OverwolfEvents);
        _provider.Type.Should().Be(DataSourceType.OverwolfEvents);

        _provider.SelectDataSource(DataSourceType.LiveClientApi);
        _provider.Type.Should().Be(DataSourceType.LiveClientApi);
    }
}
