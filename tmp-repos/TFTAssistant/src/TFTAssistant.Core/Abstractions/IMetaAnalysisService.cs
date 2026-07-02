using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Abstractions;

public interface IMetaAnalysisService
{
    Task<IEnumerable<LineupData>> GetMetaLineupsAsync(string setVersion, string tier = "all", CancellationToken cancellationToken = default);
    Task<IEnumerable<LineupTrend>> GetLineupTrendsAsync(string setVersion, string lineupId, int days = 7, CancellationToken cancellationToken = default);
    Task<MetaTierAnalysis> GetMetaTierAnalysisAsync(string setVersion, CancellationToken cancellationToken = default);
    Task ClearCacheAsync(CancellationToken cancellationToken = default);
}

public sealed class LineupTrend
{
    public string LineupId { get; init; } = "";
    public string LineupName { get; init; } = "";
    public DateTime Date { get; init; }
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
    public int AveragePlacement { get; init; }
}

public sealed class MetaTierAnalysis
{
    public string SetVersion { get; init; } = "";
    public DateTime LastUpdated { get; init; }
    public IReadOnlyDictionary<string, IEnumerable<LineupData>> TierLineups { get; init; } = new Dictionary<string, IEnumerable<LineupData>>();
    public IReadOnlyDictionary<string, MetaTierStats> TierStats { get; init; } = new Dictionary<string, MetaTierStats>();
}

public sealed class MetaTierStats
{
    public string Tier { get; init; } = "";
    public int TotalMatches { get; init; }
    public int TotalPlayers { get; init; }
    public double AverageGameTime { get; init; }
}
