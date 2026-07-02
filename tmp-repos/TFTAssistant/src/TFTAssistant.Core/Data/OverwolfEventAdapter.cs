using System.Text.Json;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Data;

public sealed class OverwolfEventAdapter : IGameDataProvider
{
    private readonly ILogger<OverwolfEventAdapter> _logger;
    private GameState? _lastState;
    private readonly object _lock = new();

    public bool IsConnected { get; private set; }
    public DataSourceType Type => DataSourceType.OverwolfEvents;
    public event EventHandler<GameStateDiff>? StateChanged;

    public OverwolfEventAdapter(ILogger<OverwolfEventAdapter> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Overwolf 事件适配器启动（等待 JS Bridge 调用）");
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    public Task<GameState?> GetFullStateAsync()
    {
        lock (_lock) { return Task.FromResult(_lastState); }
    }

    public void OnInfoUpdate(string jsonPayload)
    {
        try
        {
            var update = JsonSerializer.Deserialize<OWInfoUpdate>(jsonPayload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (update?.Info?.LiveClientData is null) return;

            IsConnected = true;
            var newState = OWInfoUpdateMapper.ToGameState(update.Info.LiveClientData);

            lock (_lock)
            {
                var diff = GameStateDiff.Compute(_lastState, newState);
                if (diff.HasChanges)
                {
                    _logger.LogDebug("游戏状态变化: {Diff}", JsonSerializer.Serialize(diff));
                    StateChanged?.Invoke(this, diff);
                }
                _lastState = newState;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Overwolf info_update 失败");
        }
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Overwolf 事件适配器启动（等待 JS Bridge 调用）");
        // 这里可以添加初始化逻辑
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        IsConnected = false;
        _logger.LogInformation("Overwolf 事件适配器停止");
        await Task.CompletedTask;
    }

    public void OnGameEvent(string jsonPayload)
    {
        try
        {
            var events = JsonSerializer.Deserialize<List<OWGameEvent>>(jsonPayload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (events is null) return;

            foreach (var e in events)
            {
                _logger.LogDebug("收到 Overwolf 事件: {Name}", e.Name);
                ProcessGameEvent(e);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Overwolf game_event 失败");
        }
    }

    private void ProcessGameEvent(OWGameEvent gameEvent)
    {
        if (gameEvent.Name == null) return;

        switch (gameEvent.Name)
        {
            case "RoundStart":
                HandleRoundStart(gameEvent);
                break;
            case "RoundEnd":
                HandleRoundEnd(gameEvent);
                break;
            case "ItemCombined":
                HandleItemCombined(gameEvent);
                break;
            case "ChampionLevelUp":
                HandleChampionLevelUp(gameEvent);
                break;
            case "AugmentPicked":
                HandleAugmentPicked(gameEvent);
                break;
            case "ChampionSold":
                HandleChampionSold(gameEvent);
                break;
            case "ChampionBought":
                HandleChampionBought(gameEvent);
                break;
            case "ChampionPlaced":
                HandleChampionPlaced(gameEvent);
                break;
            case "ChampionMoved":
                HandleChampionMoved(gameEvent);
                break;
            default:
                _logger.LogDebug("未处理的游戏事件: {Name}", gameEvent.Name);
                break;
        }
    }

    private void HandleRoundStart(OWGameEvent gameEvent)
    {
        _logger.LogInformation("回合开始: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理回合开始逻辑
    }

    private void HandleRoundEnd(OWGameEvent gameEvent)
    {
        _logger.LogInformation("回合结束: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理回合结束逻辑
    }

    private void HandleItemCombined(OWGameEvent gameEvent)
    {
        _logger.LogInformation("装备合成: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理装备合成逻辑
    }

    private void HandleChampionLevelUp(OWGameEvent gameEvent)
    {
        _logger.LogInformation("英雄升级: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理英雄升级逻辑
    }

    private void HandleAugmentPicked(OWGameEvent gameEvent)
    {
        _logger.LogInformation("选择强化符文: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理强化符文选择逻辑
    }

    private void HandleChampionSold(OWGameEvent gameEvent)
    {
        _logger.LogInformation("出售英雄: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理出售英雄逻辑
    }

    private void HandleChampionBought(OWGameEvent gameEvent)
    {
        _logger.LogInformation("购买英雄: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理购买英雄逻辑
    }

    private void HandleChampionPlaced(OWGameEvent gameEvent)
    {
        _logger.LogInformation("放置英雄: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理放置英雄逻辑
    }

    private void HandleChampionMoved(OWGameEvent gameEvent)
    {
        _logger.LogInformation("移动英雄: {Data}", JsonSerializer.Serialize(gameEvent.Data));
        // 处理移动英雄逻辑
    }
}

internal sealed class OWInfoUpdate
{
    public OWInfoData? Info { get; set; }
}

internal sealed class OWInfoData
{
    public OWLiveClientData? LiveClientData { get; set; }
}

internal sealed class OWLiveClientData
{
    public List<OWPlayer>? AllPlayers { get; set; }
    public OWActivePlayer? ActivePlayer { get; set; }
    public OWGameData? GameData { get; set; }
}

internal sealed class OWPlayer
{
    public string? SummonerName { get; set; }
    public double Health { get; set; }
    public int Placement { get; set; }
    public int TotalGold { get; set; }
    public int Level { get; set; }
}

internal sealed class OWActivePlayer : OWPlayer
{
    public int CurrentGold { get; set; }
    public int Experience { get; set; }
    public List<OWUnit>? Board { get; set; }
    public List<OWUnit>? Bench { get; set; }
    public List<OWShopUnit>? Shop { get; set; }
}

internal sealed class OWUnit
{
    public OWCharacter? Character { get; set; }
    public int StarLevel { get; set; }
    public List<OWItem>? Items { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
}

internal sealed class OWCharacter
{
    public string? Name { get; set; }
}

internal sealed class OWItem
{
    public string? Name { get; set; }
}

internal sealed class OWShopUnit
{
    public OWCharacter? Character { get; set; }
    public int Cost { get; set; }
}

internal sealed class OWGameData
{
    public double GameTime { get; set; }
    public int Round { get; set; }
    public int Stage { get; set; }
    public bool IsPvp { get; set; }
    public int SetNumber { get; set; }
}

internal sealed class OWGameEvent
{
    public string? Name { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

internal static class OWInfoUpdateMapper
{
    public static GameState ToGameState(OWLiveClientData data)
    {
        var active = data.ActivePlayer ?? new OWActivePlayer();
        var gameData = data.GameData ?? new OWGameData();
        var allPlayers = data.AllPlayers ?? new List<OWPlayer>();

        return new GameState
        {
            ActivePlayer = new ActivePlayer
            {
                SummonerName = active.SummonerName ?? "Unknown",
                Level = active.Level,
                CurrentGold = active.CurrentGold,
                Health = active.Health,
                Experience = active.Experience,
                TotalGold = active.TotalGold,
                Placement = active.Placement,
                Board = (active.Board ?? new List<OWUnit>())
                    .Where(b => b.Character?.Name != null)
                    .Select(b => new BoardUnit
                    {
                        ChampionName = b.Character!.Name,
                        StarLevel = b.StarLevel,
                        Items = b.Items?.Select(i => i.Name ?? "").ToList()
                                ?? new List<string>(),
                        Row = b.TileY,
                        Column = b.TileX
                    }).ToList(),
                Bench = (active.Bench ?? new List<OWUnit>())
                    .Select((b, i) => new BenchUnit
                    {
                        ChampionName = b.Character?.Name ?? "Unknown",
                        StarLevel = b.StarLevel,
                        Items = b.Items?.Select(i => i.Name ?? "").ToList()
                                ?? new List<string>(),
                        BenchSlot = i
                    }).ToList(),
                Shop = (active.Shop ?? new List<OWShopUnit>())
                    .Select((s, i) => new ShopUnit
                    {
                        ChampionName = s.Character?.Name ?? "Unknown",
                        Cost = s.Cost,
                        ShopSlot = i
                    }).ToList()
            },
            AllPlayers = allPlayers.Select(p => new PlayerSummary
            {
                SummonerName = p.SummonerName ?? "Unknown",
                Health = p.Health,
                Placement = p.Placement,
                TotalGold = p.TotalGold,
                Level = p.Level
            }).ToList(),
            GameInfo = new GameInfo
            {
                GameTime = gameData.GameTime,
                Round = gameData.Round,
                Stage = gameData.Stage,
                IsPvp = gameData.IsPvp,
                SetNumber = $"Set{gameData.SetNumber}"
            },
            Augments = new List<Augment>()
        };
    }
}
