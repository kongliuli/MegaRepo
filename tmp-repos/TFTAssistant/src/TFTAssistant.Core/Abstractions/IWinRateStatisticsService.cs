using TFTAssistant.Core.Models.Analytics;

namespace TFTAssistant.Core.Abstractions;

public interface IWinRateStatisticsService
{
    Task<MatchStats?> GetOverallStatisticsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<CompStats>> GetCompStatisticsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<VersionStats>> GetVersionStatisticsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task ClearCacheAsync(string playerPuuid, CancellationToken cancellationToken = default);
}
