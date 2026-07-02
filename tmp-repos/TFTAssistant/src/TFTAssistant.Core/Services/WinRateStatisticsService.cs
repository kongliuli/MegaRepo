using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Analytics;

namespace TFTAssistant.Core.Services;

public class WinRateStatisticsService : IWinRateStatisticsService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WinRateStatisticsService>? _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private const string OverallStatsCacheKeyPrefix = "OverallStats_";
    private const string CompStatsCacheKeyPrefix = "CompStats_";
    private const string VersionStatsCacheKeyPrefix = "VersionStats_";

    public WinRateStatisticsService(
        IMatchRepository matchRepository,
        IMemoryCache cache,
        ILogger<WinRateStatisticsService>? logger = null)
    {
        _matchRepository = matchRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<MatchStats?> GetOverallStatisticsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(OverallStatsCacheKeyPrefix, playerPuuid, startDate, endDate);
        
        if (_cache.TryGetValue<MatchStats>(cacheKey, out var cachedStats))
        {
            _logger?.LogDebug("Returning cached overall statistics for player: {PlayerPuuid}", playerPuuid);
            return cachedStats;
        }

        var stats = await _matchRepository.GetMatchStatsAsync(playerPuuid, startDate, endDate, cancellationToken);
        
        if (stats != null)
        {
            _cache.Set(cacheKey, stats, CacheDuration);
            _logger?.LogDebug("Cached overall statistics for player: {PlayerPuuid}", playerPuuid);
        }

        return stats;
    }

    public async Task<IEnumerable<CompStats>> GetCompStatisticsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(CompStatsCacheKeyPrefix, playerPuuid, startDate, endDate);
        
        if (_cache.TryGetValue<IEnumerable<CompStats>>(cacheKey, out var cachedStats))
        {
            _logger?.LogDebug("Returning cached comp statistics for player: {PlayerPuuid}", playerPuuid);
            return cachedStats;
        }

        var stats = await _matchRepository.GetCompStatsAsync(playerPuuid, startDate, endDate, cancellationToken);
        
        _cache.Set(cacheKey, stats, CacheDuration);
        _logger?.LogDebug("Cached comp statistics for player: {PlayerPuuid}", playerPuuid);

        return stats;
    }

    public async Task<IEnumerable<VersionStats>> GetVersionStatisticsAsync(string playerPuuid, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(VersionStatsCacheKeyPrefix, playerPuuid, startDate, endDate);
        
        if (_cache.TryGetValue<IEnumerable<VersionStats>>(cacheKey, out var cachedStats))
        {
            _logger?.LogDebug("Returning cached version statistics for player: {PlayerPuuid}", playerPuuid);
            return cachedStats;
        }

        var stats = await _matchRepository.GetVersionStatsAsync(playerPuuid, startDate, endDate, cancellationToken);
        
        _cache.Set(cacheKey, stats, CacheDuration);
        _logger?.LogDebug("Cached version statistics for player: {PlayerPuuid}", playerPuuid);

        return stats;
    }

    public Task ClearCacheAsync(string playerPuuid, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Clearing cache for player: {PlayerPuuid}", playerPuuid);
        
        var keysToRemove = new List<string>
        {
            $"{OverallStatsCacheKeyPrefix}{playerPuuid}",
            $"{CompStatsCacheKeyPrefix}{playerPuuid}",
            $"{VersionStatsCacheKeyPrefix}{playerPuuid}"
        };

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }

        return Task.CompletedTask;
    }

    private string BuildCacheKey(string prefix, string playerPuuid, DateTime? startDate, DateTime? endDate)
    {
        var startDateStr = startDate?.ToString("yyyyMMddHHmmss") ?? "min";
        var endDateStr = endDate?.ToString("yyyyMMddHHmmss") ?? "max";
        return $"{prefix}{playerPuuid}_{startDateStr}_{endDateStr}";
    }
}
