using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Data;

public sealed class CompositeGameDataProvider : IGameDataProvider
{
    private readonly LiveClientDataProvider _liveClientProvider;
    private readonly OverwolfEventAdapter _overwolfAdapter;
    private readonly ILogger<CompositeGameDataProvider> _logger;
    private Timer? _healthCheckTimer;
    private IGameDataProvider? _activeProvider;
    private readonly object _lock = new();
    private bool _manualOverride = false;
    private DataSourceType? _manualSelectedType;
    private CancellationTokenSource? _cts;
    private GameState? _lastState;

    private static readonly TimeSpan HealthCheckInterval = TimeSpan.FromSeconds(2);

    public bool IsConnected => _activeProvider?.IsConnected ?? false;
    public DataSourceType Type => _activeProvider?.Type ?? DataSourceType.LiveClientApi;

    public event EventHandler<GameStateDiff>? StateChanged;

    public CompositeGameDataProvider(
        LiveClientDataProvider liveClientProvider,
        OverwolfEventAdapter overwolfAdapter,
        ILogger<CompositeGameDataProvider> logger)
    {
        _liveClientProvider = liveClientProvider;
        _overwolfAdapter = overwolfAdapter;
        _logger = logger;

        _liveClientProvider.StateChanged += OnProviderStateChanged;
        _overwolfAdapter.StateChanged += OnProviderStateChanged;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Composite Game Data Provider 启动");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        _liveClientProvider.StartAsync(ct);
        _overwolfAdapter.StartAsync(ct);

        _healthCheckTimer = new Timer(
            async _ => await HealthCheckAsync(),
            null,
            TimeSpan.Zero,
            HealthCheckInterval);

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _healthCheckTimer?.Dispose();
        _healthCheckTimer = null;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        _liveClientProvider.StopAsync();
        _overwolfAdapter.StopAsync();

        _activeProvider = null;
        _logger.LogInformation("Composite Game Data Provider 停止");
        return Task.CompletedTask;
    }

    public Task<GameState?> GetFullStateAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_lastState);
        }
    }

    public void SelectDataSource(DataSourceType type)
    {
        lock (_lock)
        {
            _manualOverride = true;
            _manualSelectedType = type;
            SwitchToProvider(type);
        }
    }

    public void ResetToAutomatic()
    {
        lock (_lock)
        {
            _manualOverride = false;
            _manualSelectedType = null;
            _logger.LogInformation("已恢复自动数据源选择模式");
        }
    }

    private async Task HealthCheckAsync()
    {
        if (_cts?.IsCancellationRequested ?? true)
            return;

        lock (_lock)
        {
            if (_manualOverride)
                return;

            if (_liveClientProvider.IsConnected)
            {
                if (_activeProvider != _liveClientProvider)
                {
                    SwitchToProvider(DataSourceType.LiveClientApi);
                }
            }
            else if (_overwolfAdapter.IsConnected)
            {
                if (_activeProvider != _overwolfAdapter)
                {
                    SwitchToProvider(DataSourceType.OverwolfEvents);
                }
            }
            else
            {
                if (_activeProvider != null)
                {
                    _activeProvider = null;
                    _logger.LogWarning("所有数据源均不可用");
                }
            }
        }
    }

    private void SwitchToProvider(DataSourceType type)
    {
        IGameDataProvider? newProvider = type switch
        {
            DataSourceType.LiveClientApi => _liveClientProvider,
            DataSourceType.OverwolfEvents => _overwolfAdapter,
            _ => null
        };

        if (newProvider != null && newProvider != _activeProvider)
        {
            _activeProvider = newProvider;
            _logger.LogInformation("已切换到数据源: {Type}", type);
        }
    }

    private void OnProviderStateChanged(object? sender, GameStateDiff diff)
    {
        var provider = sender as IGameDataProvider;
        if (provider == null || provider != _activeProvider)
            return;

        lock (_lock)
        {
            if (_activeProvider != null)
            {
                var stateTask = _activeProvider.GetFullStateAsync();
                stateTask.Wait();
                _lastState = stateTask.Result;
            }
        }

        StateChanged?.Invoke(this, diff);
    }
}
