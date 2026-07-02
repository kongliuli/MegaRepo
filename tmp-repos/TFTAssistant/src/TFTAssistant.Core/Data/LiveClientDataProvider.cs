using System.Net.Http.Json;
using System.Net.Security;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Data;

public sealed class LiveClientDataProvider : IGameDataProvider
{
    private const string BASE_URL = "https://127.0.0.1:2999/liveclientdata";
    private readonly HttpClient _http;
    private readonly ILogger<LiveClientDataProvider> _logger;
    private Timer? _pollTimer;
    private GameState? _lastState;
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    public bool IsConnected { get; private set; }
    public DataSourceType Type => DataSourceType.LiveClientApi;

    public event EventHandler<GameStateDiff>? StateChanged;

    public LiveClientDataProvider(ILogger<LiveClientDataProvider> logger)
    {
        _logger = logger;
        
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (
                HttpRequestMessage _,
                System.Security.Cryptography.X509Certificates.X509Certificate2? _,
                System.Security.Cryptography.X509Certificates.X509Chain? _,
                SslPolicyErrors _) => true
        };
        
        _http = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(2)
        };
    }

    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Live Client Data Provider 启动");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _pollTimer = new Timer(
            async _ => await PollAsync(),
            null,
            TimeSpan.Zero,
            PollInterval);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _pollTimer?.Dispose();
        _pollTimer = null;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsConnected = false;
        _logger.LogInformation("Live Client Data Provider 停止");
        return Task.CompletedTask;
    }

    public Task<GameState?> GetFullStateAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_lastState);
        }
    }

    private int _reconnectAttempts = 0;
    private const int MaxReconnectAttempts = 5;
    private DateTime _lastReconnectAttempt = DateTime.MinValue;
    private const int ReconnectCooldownMs = 1000;

    private async Task PollAsync()
    {
        if (_cts?.IsCancellationRequested ?? true)
            return;

        try
        {
            // 检查重连冷却
            if (_reconnectAttempts > 0 && (DateTime.Now - _lastReconnectAttempt).TotalMilliseconds < ReconnectCooldownMs)
            {
                await Task.Delay(ReconnectCooldownMs, _cts.Token);
            }

            var json = await _http.GetStringAsync(
                $"{BASE_URL}/allgamedata", _cts.Token);

            var dto = JsonSerializer.Deserialize<LiveGameDataDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto is null || dto.ActivePlayer is null || dto.GameData is null)
            {
                _logger.LogDebug("Live Client API 返回数据不完整");
                return;
            }

            var newState = dto.ToGameState();
            
            // 验证游戏状态数据的有效性
            if (!newState.IsValid())
            {
                _logger.LogWarning("Live Client API 返回数据无效，跳过处理");
                return;
            }

            lock (_lock)
            {
                if (!IsConnected)
                {
                    _logger.LogInformation("已连接到 Live Client API");
                    IsConnected = true;
                    _reconnectAttempts = 0; // 重置重连尝试
                }

                var diff = GameStateDiff.Compute(_lastState, newState);

                if (diff.HasBoardChanged || diff.HasBenchChanged
                    || diff.HasShopChanged || diff.HasGoldChanged
                    || diff.HasHealthChanged || diff.HasLevelChanged
                    || diff.HasRoundChanged || diff.HasAugmentPicked)
                {
                    try
                    {
                        StateChanged?.Invoke(this, diff);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "状态变化事件处理失败");
                    }
                }

                _lastState = newState;
            }
        }
        catch (HttpRequestException ex)
        {
            if (IsConnected)
            {
                _logger.LogWarning(ex, "Live Client API 连接失败（游戏可能未运行或API不可用）");
                IsConnected = false;
            }
            else
            {
                _reconnectAttempts++;
                _lastReconnectAttempt = DateTime.Now;
                if (_reconnectAttempts <= MaxReconnectAttempts)
                {
                    _logger.LogDebug("尝试重连 Live Client API ({Attempt}/{Max})");
                }
                else if (_reconnectAttempts == MaxReconnectAttempts + 1)
                {
                    _logger.LogInformation("达到最大重连尝试次数，将继续尝试但降低频率");
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Live Client API 返回数据解析失败，数据格式可能已变更");
        }
        catch (OperationCanceledException)
        {
            // 正常取消，无需记录
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Live Client Data Provider 轮询异常");
        }
    }
}
