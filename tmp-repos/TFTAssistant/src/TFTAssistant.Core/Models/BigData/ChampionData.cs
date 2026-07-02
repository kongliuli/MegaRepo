namespace TFTAssistant.Core.Models.BigData;

public sealed class ChampionData
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string SetVersion { get; init; } = "";
    public int Cost { get; init; }
    public IReadOnlyList<string> Traits { get; init; } = new List<string>();
    public string ImageUrl { get; init; } = "";
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public double AveragePlacement { get; init; }
    public int MatchCount { get; init; }
    public DateTime LastUpdated { get; init; }
    public bool IsHistoricalData { get; init; } = true;
    public IReadOnlyDictionary<string, double> WinRateByTier { get; init; } = new Dictionary<string, double>();
    public IReadOnlyDictionary<string, double> PickRateByTier { get; init; } = new Dictionary<string, double>();
    public IReadOnlyList<ChampionTrend> Trends { get; init; } = new List<ChampionTrend>();
}

public sealed class ChampionTrend
{
    public DateTime Date { get; init; }
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
    public double AveragePlacement { get; init; }
}
