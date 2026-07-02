using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Abstractions;

public interface ILineupAnalysisService
{
    // 1. 阵容强度评分算法
    Task<double> CalculateLineupStrengthAsync(string setVersion, string lineupId, CancellationToken cancellationToken = default);
    
    // 2. 装备优先级算法
    Task<IEnumerable<EquipmentPriority>> GetEquipmentPrioritiesAsync(string setVersion, string lineupId, CancellationToken cancellationToken = default);
    
    // 3. 阵容克制关系分析
    Task<CounterAnalysis> GetCounterAnalysisAsync(string setVersion, string lineupId, CancellationToken cancellationToken = default);
    
    // 缓存管理
    Task ClearCacheAsync(CancellationToken cancellationToken = default);
}
