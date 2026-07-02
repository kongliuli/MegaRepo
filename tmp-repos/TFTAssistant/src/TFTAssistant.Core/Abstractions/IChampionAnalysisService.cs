using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Abstractions;

public interface IChampionAnalysisService
{
    Task<ChampionAnalysis> GetChampionAnalysisAsync(string setVersion, string championId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChampionData>> GetChampionsByTierAsync(string setVersion, string tier, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChampionTrend>> GetChampionTrendsAsync(string setVersion, string championId, int days = 7, CancellationToken cancellationToken = default);
    Task<IEnumerable<EquipmentRecommendation>> GetChampionEquipmentRecommendationsAsync(string setVersion, string championId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChampionLineupPerformance>> GetChampionLineupPerformanceAsync(string setVersion, string championId, CancellationToken cancellationToken = default);
    Task ClearCacheAsync(CancellationToken cancellationToken = default);
}
