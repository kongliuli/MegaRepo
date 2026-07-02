using System.Text.Json;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Data;

namespace TFTAssistant.Overwolf.Bridge;

public sealed class OWBridge : IDisposable
{
    private readonly ILogger<OWBridge> _logger;
    private readonly CancellationTokenSource _cts = new();
    private readonly OverwolfEventAdapter _eventAdapter;

    public OWBridge(ILogger<OWBridge> logger, OverwolfEventAdapter eventAdapter)
    {
        _logger = logger;
        _eventAdapter = eventAdapter;
    }

    public async Task OnMessageReceivedAsync(string jsonMessage)
    {
        try
        {
            var msg = JsonSerializer.Deserialize<OWMessage>(jsonMessage,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (msg is null || msg.Type is null) return;

            switch (msg.Type)
            {
                case MessageTypes.OverwolfInfoUpdate:
                    await HandleInfoUpdate(msg.Payload);
                    break;

                case MessageTypes.OverwolfGameEvent:
                    await HandleGameEvent(msg.Payload);
                    break;

                case MessageTypes.UiReady:
                    await SendCurrentStateAsync();
                    break;

                case MessageTypes.RequestRecommendations:
                    await SendRecommendationsAsync();
                    break;

                case MessageTypes.ToggleOverlay:
                    await HandleToggleOverlay();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Bridge 消息失败");
        }
    }

    private async Task HandleInfoUpdate(object? payload)
    {
        try
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            _eventAdapter.OnInfoUpdate(jsonPayload);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Overwolf 信息更新失败");
        }
    }

    private async Task HandleGameEvent(object? payload)
    {
        try
        {
            var jsonPayload = JsonSerializer.Serialize(payload);
            _eventAdapter.OnGameEvent(jsonPayload);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Overwolf 游戏事件失败");
        }
    }

    private async Task SendCurrentStateAsync()
    {
        try
        {
            var currentState = await _eventAdapter.GetFullStateAsync();
            if (currentState != null)
            {
                var jsonState = JsonSerializer.Serialize(currentState);
                await SendToUIAsync(MessageTypes.FullState, jsonState);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送当前状态失败");
        }
    }

    private async Task SendRecommendationsAsync()
    {
        _logger.LogDebug("发送推荐数据到 UI");
        // 这里需要实现推荐数据的生成和发送
        await Task.CompletedTask;
    }

    private async Task HandleToggleOverlay()
    {
        _logger.LogDebug("切换覆盖层显示状态");
        // 这里需要实现覆盖层的显示/隐藏逻辑
        await Task.CompletedTask;
    }

    public async Task SendToUIAsync(string type, string json)
    {
        _logger.LogDebug("发送到 UI: {Type}", type);
        // 这里需要实现与前端的通信机制
        // 暂时使用日志模拟
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
