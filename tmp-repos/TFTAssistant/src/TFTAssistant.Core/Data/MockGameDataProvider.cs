using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Data;

public sealed class MockGameDataProvider : IGameDataProvider
{
    private readonly ILogger<MockGameDataProvider> _logger;
    private Timer? _pollTimer;
    private GameState? _lastState;
    private readonly object _lock = new();
    private CancellationTokenSource? _cts;
    private int _updateCount = 0;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private static readonly string[] ChampionNames = new[]
    {
        "Ahri", "Ezreal", "Lux", "Yasuo", "Zed", "Katarina", "LeeSin", "Vayne",
        "Jinx", "Caitlyn", "Ashe", "Draven", "MissFortune", "Tristana", "Kindred"
    };
    private static readonly string[] ItemNames = new[]
    {
        "InfinityEdge", "RabadonsDeathcap", "GuinsoosRageblade", "Bloodthirster",
        "GuardianAngel", "BrambleVest", "DragonsClaw", "WarmogsArmor"
    };
    private static readonly string[] SummonerNames = new[]
    {
        "Player1", "Player2", "Player3", "Player4",
        "Player5", "Player6", "Player7", "Player8"
    };

    public bool IsConnected { get; private set; }
    public DataSourceType Type => DataSourceType.Mock;

    public event EventHandler<GameStateDiff>? StateChanged;

    public MockGameDataProvider(ILogger<MockGameDataProvider> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Mock Game Data Provider 启动");
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        IsConnected = true;
        _pollTimer = new Timer(
            async _ => await UpdateGameStateAsync(),
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
        _logger.LogInformation("Mock Game Data Provider 停止");
        return Task.CompletedTask;
    }

    public Task<GameState?> GetFullStateAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_lastState);
        }
    }

    private async Task UpdateGameStateAsync()
    {
        if (_cts?.IsCancellationRequested ?? true)
            return;

        try
        {
            _updateCount++;
            var newState = GenerateMockGameState();

            lock (_lock)
            {
                var diff = GameStateDiff.Compute(_lastState, newState);

                if (diff.HasBoardChanged || diff.HasBenchChanged
                    || diff.HasShopChanged || diff.HasGoldChanged
                    || diff.HasHealthChanged || diff.HasLevelChanged
                    || diff.HasRoundChanged || diff.HasAugmentPicked)
                {
                    StateChanged?.Invoke(this, diff);
                    _logger.LogDebug("Mock 游戏状态已更新 (第 {Count} 次)", _updateCount);
                }

                _lastState = newState;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mock Game Data Provider 更新异常");
        }

        await Task.CompletedTask;
    }

    private GameState GenerateMockGameState()
    {
        var random = new Random();
        int stage = 2 + (_updateCount / 3);
        int round = 1 + (_updateCount % 3);
        int level = Math.Min(9, 4 + (_updateCount / 2));
        int gold = 10 + (_updateCount % 30);
        double health = Math.Max(1, 100 - (_updateCount * 2));

        var boardUnits = GenerateBoardUnits(random, level);
        var benchUnits = GenerateBenchUnits(random);
        var shopUnits = GenerateShopUnits(random);
        var allPlayers = GeneratePlayerSummaries(random);
        var augments = GenerateAugments(random);

        return new GameState
        {
            ActivePlayer = new ActivePlayer
            {
                SummonerName = SummonerNames[0],
                Level = level,
                CurrentGold = gold,
                Health = health,
                Experience = (level - 1) * 100 + random.Next(50),
                TotalGold = 100 + _updateCount * 5,
                Placement = 1,
                Board = boardUnits,
                Bench = benchUnits,
                Shop = shopUnits
            },
            AllPlayers = allPlayers,
            GameInfo = new GameInfo
            {
                GameTime = _updateCount * 5,
                Round = round,
                Stage = stage,
                IsPvp = round > 1,
                SetNumber = "Set13"
            },
            Augments = augments
        };
    }

    private IReadOnlyList<BoardUnit> GenerateBoardUnits(Random random, int level)
    {
        var units = new List<BoardUnit>();
        int boardSize = Math.Min(level, 7);

        for (int i = 0; i < boardSize; i++)
        {
            int row = i / 4;
            int col = i % 4;
            var items = new List<string>();
            if (random.NextDouble() > 0.5)
                items.Add(ItemNames[random.Next(ItemNames.Length)]);
            if (random.NextDouble() > 0.7)
                items.Add(ItemNames[random.Next(ItemNames.Length)]);

            units.Add(new BoardUnit
            {
                ChampionName = ChampionNames[random.Next(ChampionNames.Length)],
                StarLevel = random.Next(1, 4),
                Items = items,
                Row = row,
                Column = col,
                IsMainCarry = i == 0
            });
        }

        return units;
    }

    private IReadOnlyList<BenchUnit> GenerateBenchUnits(Random random)
    {
        var units = new List<BenchUnit>();
        int benchSize = random.Next(3, 7);

        for (int i = 0; i < benchSize; i++)
        {
            units.Add(new BenchUnit
            {
                ChampionName = ChampionNames[random.Next(ChampionNames.Length)],
                StarLevel = random.Next(1, 3),
                Items = new List<string>(),
                BenchSlot = i
            });
        }

        return units;
    }

    private IReadOnlyList<ShopUnit> GenerateShopUnits(Random random)
    {
        var units = new List<ShopUnit>();

        for (int i = 0; i < 5; i++)
        {
            units.Add(new ShopUnit
            {
                ChampionName = ChampionNames[random.Next(ChampionNames.Length)],
                Cost = random.Next(1, 6),
                ShopSlot = i
            });
        }

        return units;
    }

    private IReadOnlyList<PlayerSummary> GeneratePlayerSummaries(Random random)
    {
        var players = new List<PlayerSummary>();

        for (int i = 0; i < 8; i++)
        {
            players.Add(new PlayerSummary
            {
                SummonerName = SummonerNames[i],
                Health = Math.Max(1, 100 - (random.Next(_updateCount * 2))),
                Placement = i + 1,
                TotalGold = 50 + random.Next(100),
                Level = Math.Min(9, 4 + random.Next(3))
            });
        }

        return players;
    }

    private IReadOnlyList<Augment> GenerateAugments(Random random)
    {
        var augments = new List<Augment>();
        int augmentCount = Math.Min(3, _updateCount / 4);

        for (int i = 0; i < augmentCount; i++)
        {
            augments.Add(new Augment
            {
                Id = $"augment_{i + 1}",
                Name = $"Augment {i + 1}",
                Description = $"This is augment {i + 1} description",
                Stage = i + 1
            });
        }

        return augments;
    }
}
