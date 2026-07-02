using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Abstractions;

public interface IBigDataService
{
    // 元数据操作
    Task<MetaData?> GetMetaDataAsync(string setVersion, CancellationToken cancellationToken = default);
    Task SaveMetaDataAsync(MetaData metaData, CancellationToken cancellationToken = default);
    
    // 阵容数据操作
    Task<IEnumerable<LineupData>> GetLineupDataAsync(string setVersion, CancellationToken cancellationToken = default);
    Task<IEnumerable<LineupData>> GetLineupsAsync(string setVersion, string tier = "all", CancellationToken cancellationToken = default);
    Task SaveLineupDataAsync(LineupData lineupData, CancellationToken cancellationToken = default);
    Task SaveLineupDataBatchAsync(IEnumerable<LineupData> lineups, CancellationToken cancellationToken = default);
    
    // 装备数据操作
    Task<IEnumerable<EquipmentData>> GetEquipmentDataAsync(string setVersion, bool? isComponent = null, CancellationToken cancellationToken = default);
    Task SaveEquipmentDataAsync(EquipmentData equipmentData, CancellationToken cancellationToken = default);
    Task SaveEquipmentDataBatchAsync(IEnumerable<EquipmentData> equipmentList, CancellationToken cancellationToken = default);
    
    // 英雄数据操作
    Task<IEnumerable<ChampionData>> GetChampionDataAsync(string setVersion, int? cost = null, CancellationToken cancellationToken = default);
    Task<ChampionData?> GetChampionDataByIdAsync(string setVersion, string championId, CancellationToken cancellationToken = default);
    Task SaveChampionDataAsync(ChampionData championData, CancellationToken cancellationToken = default);
    Task SaveChampionDataBatchAsync(IEnumerable<ChampionData> champions, CancellationToken cancellationToken = default);
    
    // 英雄分析操作
    Task<IEnumerable<ChampionData>> GetChampionsByTierAsync(string setVersion, string tier, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChampionTrend>> GetChampionTrendsAsync(string setVersion, string championId, int days = 7, CancellationToken cancellationToken = default);
    Task<ChampionAnalysis> GetChampionAnalysisAsync(string setVersion, string championId, CancellationToken cancellationToken = default);
    
    // 装备推荐操作
    Task<IEnumerable<EquipmentRecommendation>> GetChampionEquipmentRecommendationsAsync(string setVersion, string championId, CancellationToken cancellationToken = default);
    
    // 阵容表现分析操作
    Task<IEnumerable<ChampionLineupPerformance>> GetChampionLineupPerformanceAsync(string setVersion, string championId, CancellationToken cancellationToken = default);
    
    // Meta 分析操作
    Task<IEnumerable<LineupTrend>> GetLineupTrendsAsync(string setVersion, string lineupId, int days = 7, CancellationToken cancellationToken = default);
    Task<MetaTierAnalysis> GetMetaTierAnalysisAsync(string setVersion, CancellationToken cancellationToken = default);
    
    // 缓存管理
    Task ClearCacheAsync(CancellationToken cancellationToken = default);
    Task<int> GetCacheSizeAsync(CancellationToken cancellationToken = default);
    
    // 版本管理
    Task<string?> GetLatestVersionAsync(string setVersion, CancellationToken cancellationToken = default);
    Task<ConsistencyCheckResult> CheckDataConsistencyAsync(string setVersion, CancellationToken cancellationToken = default);
}

public sealed class ChampionAnalysis
{
    public string ChampionId { get; init; } = "";
    public string ChampionName { get; init; } = "";
    public string SetVersion { get; init; } = "";
    public double OverallWinRate { get; init; }
    public double OverallPickRate { get; init; }
    public double AveragePlacement { get; init; }
    public int MatchCount { get; init; }
    public IReadOnlyDictionary<string, double> WinRateByTier { get; init; } = new Dictionary<string, double>();
    public IReadOnlyDictionary<string, double> PickRateByTier { get; init; } = new Dictionary<string, double>();
    public IReadOnlyList<ChampionTrend> Trends { get; init; } = new List<ChampionTrend>();
    public DateTime LastUpdated { get; init; }
    public bool IsHistoricalData { get; init; } = true;
}

public sealed class EquipmentRecommendation
{
    public string EquipmentId { get; init; } = "";
    public string EquipmentName { get; init; } = "";
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
    public int Priority { get; init; }
}

public sealed class ChampionLineupPerformance
{
    public string LineupId { get; init; } = "";
    public string LineupName { get; init; } = "";
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
    public double AveragePlacement { get; init; }
    public int SynergyScore { get; init; }
}

public sealed class ConsistencyCheckResult
{
    public bool IsConsistent { get; init; }
    public string? ErrorMessage { get; init; }
    public int ChampionsCount { get; init; }
    public int LineupsCount { get; init; }
    public int EquipmentCount { get; init; }
}
