using TFTAssistant.Core.Models.Analytics;

namespace TFTAssistant.Core.Abstractions;

public interface IMatchRepository
{
    Task SaveMatchRecordAsync(MatchRecord matchRecord, CancellationToken cancellationToken = default);
    Task<MatchRecord?> GetMatchRecordByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MatchRecord>> GetMatchRecordsAsync(int limit = 100, int offset = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<MatchRecord>> GetMatchRecordsByPlayerAsync(string playerPuuid, int limit = 100, int offset = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<MatchRecord>> GetMatchRecordsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<MatchStats?> GetMatchStatsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<CompStats>> GetCompStatsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<VersionStats>> GetVersionStatsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task SaveMatchDetailAsync(MatchDetail matchDetail, CancellationToken cancellationToken = default);
    Task<MatchDetail?> GetMatchDetailByMatchIdAsync(string matchRecordId, CancellationToken cancellationToken = default);
    Task SaveRoundSnapshotAsync(RoundSnapshot roundSnapshot, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoundSnapshot>> GetRoundSnapshotsByMatchIdAsync(string matchRecordId, CancellationToken cancellationToken = default);
    Task DeleteMatchRecordAsync(string id, CancellationToken cancellationToken = default);
}
