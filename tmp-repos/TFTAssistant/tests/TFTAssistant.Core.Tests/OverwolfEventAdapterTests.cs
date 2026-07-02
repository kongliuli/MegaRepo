using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Tests;

public class OverwolfEventAdapterTests
{
    private readonly Mock<ILogger<OverwolfEventAdapter>> _mockLogger;
    private readonly OverwolfEventAdapter _adapter;

    public OverwolfEventAdapterTests()
    {
        _mockLogger = new Mock<ILogger<OverwolfEventAdapter>>();
        _adapter = new OverwolfEventAdapter(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        _adapter.IsConnected.Should().BeFalse();
        _adapter.Type.Should().Be(DataSourceType.OverwolfEvents);
    }

    [Fact]
    public async Task StartAsync_ShouldCompleteSuccessfully()
    {
        var result = await _adapter.StartAsync(CancellationToken.None);

        result.Should().Be(Task.CompletedTask);
    }

    [Fact]
    public async Task StopAsync_ShouldSetIsConnectedToFalse()
    {
        await _adapter.StopAsync();

        _adapter.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task GetFullStateAsync_ShouldReturnNullInitially()
    {
        var state = await _adapter.GetFullStateAsync();

        state.Should().BeNull();
    }

    [Fact]
    public void OnInfoUpdate_WithValidJson_ShouldSetIsConnectedToTrue()
    {
        var validJson = @"{
            ""info"": {
                ""liveClientData"": {
                    ""activePlayer"": {
                        ""summonerName"": ""TestPlayer"",
                        ""health"": 100,
                        ""level"": 1
                    },
                    ""gameData"": {
                        ""gameTime"": 0,
                        ""round"": 1,
                        ""stage"": 1,
                        ""isPvp"": false,
                        ""setNumber"": 15
                    }
                }
            }
        }";

        _adapter.OnInfoUpdate(validJson);

        _adapter.IsConnected.Should().BeTrue();
    }

    [Fact]
    public void OnInfoUpdate_WithValidJson_ShouldTriggerStateChanged()
    {
        var eventTriggered = false;
        _adapter.StateChanged += (sender, args) => eventTriggered = true;

        var validJson = @"{
            ""info"": {
                ""liveClientData"": {
                    ""activePlayer"": {
                        ""summonerName"": ""TestPlayer"",
                        ""health"": 100,
                        ""level"": 1
                    },
                    ""gameData"": {
                        ""gameTime"": 0,
                        ""round"": 1,
                        ""stage"": 1,
                        ""isPvp"": false,
                        ""setNumber"": 15
                    }
                }
            }
        }";

        _adapter.OnInfoUpdate(validJson);

        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public void OnInfoUpdate_WithInvalidJson_ShouldNotThrow()
    {
        var invalidJson = "invalid json {{{";

        var action = () => _adapter.OnInfoUpdate(invalidJson);

        action.Should().NotThrow();
    }

    [Fact]
    public void OnInfoUpdate_WithNullLiveClientData_ShouldNotSetIsConnected()
    {
        var jsonWithNullData = @"{
            ""info"": {
                ""liveClientData"": null
            }
        }";

        _adapter.OnInfoUpdate(jsonWithNullData);

        _adapter.IsConnected.Should().BeFalse();
    }

    [Fact]
    public void OnGameEvent_WithValidJson_ShouldNotThrow()
    {
        var validEventJson = @"[
            { ""name"": ""TestEvent"", ""data"": {} }
        ]";

        var action = () => _adapter.OnGameEvent(validEventJson);

        action.Should().NotThrow();
    }

    [Fact]
    public void OnGameEvent_WithInvalidJson_ShouldNotThrow()
    {
        var invalidJson = "invalid json {{{";

        var action = () => _adapter.OnGameEvent(invalidJson);

        action.Should().NotThrow();
    }

    [Fact]
    public async Task GetFullStateAsync_AfterInfoUpdate_ShouldReturnState()
    {
        var validJson = @"{
            ""info"": {
                ""liveClientData"": {
                    ""activePlayer"": {
                        ""summonerName"": ""TestPlayer"",
                        ""health"": 100,
                        ""level"": 1
                    },
                    ""gameData"": {
                        ""gameTime"": 0,
                        ""round"": 1,
                        ""stage"": 1,
                        ""isPvp"": false,
                        ""setNumber"": 15
                    }
                }
            }
        }";

        _adapter.OnInfoUpdate(validJson);
        var state = await _adapter.GetFullStateAsync();

        state.Should().NotBeNull();
        state!.ActivePlayer.Should().NotBeNull();
        state.ActivePlayer.SummonerName.Should().Be("TestPlayer");
    }
}
