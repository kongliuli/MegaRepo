using System.Text.Json.Serialization;
using TFTAssistant.Core.Models.Game;

namespace TFTAssistant.Core.Data;

public sealed class LiveGameDataDto
{
    [JsonPropertyName("allPlayers")]
    public List<LivePlayerDto> AllPlayers { get; set; } = new();

    [JsonPropertyName("activePlayer")]
    public LiveActivePlayerDto? ActivePlayer { get; set; }

    [JsonPropertyName("gameData")]
    public LiveGameInfoDto? GameData { get; set; }

    public GameState ToGameState()
    {
        var active = ActivePlayer!;
        var gameData = GameData!;

        return new GameState
        {
            ActivePlayer = new ActivePlayer
            {
                SummonerName = active.SummonerName,
                Level = active.Level,
                CurrentGold = active.CurrentGold,
                Health = active.Health,
                Experience = active.Experience,
                TotalGold = active.TotalGold,
                Placement = active.Placement,
                Board = active.Board.Select(b => new BoardUnit
                {
                    ChampionName = b.Character!.Name,
                    StarLevel = b.StarLevel,
                    Items = b.Items?.Select(i => i.Name ?? "").ToList()
                            ?? new List<string>(),
                    Row = b.TileY,
                    Column = b.TileX
                }).ToList(),
                Bench = active.Bench.Select((b, i) => new BenchUnit
                {
                    ChampionName = b.Character?.Name ?? "Unknown",
                    StarLevel = b.StarLevel,
                    Items = b.Items?.Select(i => i.Name ?? "").ToList()
                            ?? new List<string>(),
                    BenchSlot = i
                }).ToList(),
                Shop = active.Shop.Select((s, i) => new ShopUnit
                {
                    ChampionName = s.Character?.Name ?? "Unknown",
                    Cost = s.Cost,
                    ShopSlot = i
                }).ToList()
            },
            AllPlayers = AllPlayers.Select(p => new PlayerSummary
            {
                SummonerName = p.SummonerName,
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

public class LivePlayerDto
{
    public string SummonerName { get; set; } = "";
    public double Health { get; set; }
    public int Placement { get; set; }
    public int TotalGold { get; set; }
    public int Level { get; set; }
}

public sealed class LiveActivePlayerDto : LivePlayerDto
{
    public int CurrentGold { get; set; }
    public int Experience { get; set; }
    public List<LiveUnitDto> Board { get; set; } = new();
    public List<LiveUnitDto> Bench { get; set; } = new();
    public List<LiveShopUnitDto> Shop { get; set; } = new();
}

public sealed class LiveUnitDto
{
    public LiveCharacterDto? Character { get; set; }
    public int StarLevel { get; set; }
    public List<LiveItemDto>? Items { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
}

public sealed class LiveCharacterDto
{
    public string? Name { get; set; }
}

public sealed class LiveItemDto
{
    public string? Name { get; set; }
}

public sealed class LiveShopUnitDto
{
    public LiveCharacterDto? Character { get; set; }
    public int Cost { get; set; }
}

public sealed class LiveGameInfoDto
{
    public double GameTime { get; set; }
    public int Round { get; set; }
    public int Stage { get; set; }
    public bool IsPvp { get; set; }
    public int SetNumber { get; set; }
}
