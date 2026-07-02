namespace TFTAssistant.Core.Models.BigData;

public sealed class LineupData
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string SetVersion { get; init; } = "";
    public IReadOnlyList<string> Champions { get; init; } = new List<string>();
    public IReadOnlyList<string> Traits { get; init; } = new List<string>();
    public IReadOnlyList<ChampionItem> ChampionItems { get; init; } = new List<ChampionItem>();
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
    public int AveragePlacement { get; init; }
    public DateTime LastUpdated { get; init; }
}

public sealed class ChampionItem
{
    public string ChampionId { get; init; } = "";
    public IReadOnlyList<string> Items { get; init; } = new List<string>();
}
