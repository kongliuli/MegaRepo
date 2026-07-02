namespace TFTAssistant.Core.Abstractions;

public interface IGameDataSource
{
    Task StartAsync(CancellationToken ct);
    Task StopAsync();
    bool IsConnected { get; }
    DataSourceType Type { get; }
}

public enum DataSourceType
{
    LiveClientApi,
    OverwolfEvents,
    Mock
}
