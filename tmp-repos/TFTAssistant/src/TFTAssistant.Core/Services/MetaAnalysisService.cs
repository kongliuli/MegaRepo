using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Services;

public class MetaAnalysisService : IMetaAnalysisService
{
    private readonly IBigDataService _bigDataService;
    private readonly ILineupAnalysisService _lineupAnalysisService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MetaAnalysisService>? _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private const string MetaLineupsCacheKeyPrefix = "MetaLineups_";
    private const string LineupTrendsCacheKeyPrefix = "LineupTrends_";
    private const string MetaTierAnalysisCacheKeyPrefix = "MetaTierAnalysis_";

    public MetaAnalysisService(
        IBigDataService bigDataService,
        ILineupAnalysisService lineupAnalysisService,
        IMemoryCache cache,
        ILogger<MetaAnalysisService>? logger = null)
    {
        _bigDataService = bigDataService;
        _lineupAnalysisService = lineupAnalysisService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<LineupData>> GetMetaLineupsAsync(string setVersion, string tier = "all", CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{MetaLineupsCacheKeyPrefix}{setVersion}_{tier}";
        
        if (_cache.TryGetValue<IEnumerable<LineupData>>(cacheKey, out var cachedLineups))
        {
            _logger?.LogDebug("Returning cached meta lineups for set {SetVersion} and tier {Tier}", setVersion, tier);
            return cachedLineups;
        }

        var lineups = await _bigDataService.GetLineupsAsync(setVersion, tier, cancellationToken);
        var rankedLineups = await RankLineupsAsync(lineups, setVersion, cancellationToken);
        
        _cache.Set(cacheKey, rankedLineups, CacheDuration);
        _logger?.LogDebug("Cached meta lineups for set {SetVersion} and tier {Tier}", setVersion, tier);

        return rankedLineups;
    }

    public async Task<IEnumerable<LineupTrend>> GetLineupTrendsAsync(string setVersion, string lineupId, int days = 7, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{LineupTrendsCacheKeyPrefix}{setVersion}_{lineupId}_{days}";
        
        if (_cache.TryGetValue<IEnumerable<LineupTrend>>(cacheKey, out var cachedTrends))
        {
            _logger?.LogDebug("Returning cached lineup trends for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);
            return cachedTrends;
        }

        var trends = await _bigDataService.GetLineupTrendsAsync(setVersion, lineupId, days, cancellationToken);
        
        _cache.Set(cacheKey, trends, CacheDuration);
        _logger?.LogDebug("Cached lineup trends for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);

        return trends;
    }

    public async Task<MetaTierAnalysis> GetMetaTierAnalysisAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{MetaTierAnalysisCacheKeyPrefix}{setVersion}";
        
        if (_cache.TryGetValue<MetaTierAnalysis>(cacheKey, out var cachedAnalysis))
        {
            _logger?.LogDebug("Returning cached meta tier analysis for set {SetVersion}", setVersion);
            return cachedAnalysis;
        }

        var analysis = await _bigDataService.GetMetaTierAnalysisAsync(setVersion, cancellationToken);
        
        _cache.Set(cacheKey, analysis, CacheDuration);
        _logger?.LogDebug("Cached meta tier analysis for set {SetVersion}", setVersion);

        return analysis;
    }

    public Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Clearing meta analysis cache");
        
        // In a real implementation, we would clear all meta-related cache keys
        // For simplicity, we'll just let the cache expire naturally
        
        return Task.CompletedTask;
    }

    private async Task<IEnumerable<LineupData>> RankLineupsAsync(IEnumerable<LineupData> lineups, string setVersion, CancellationToken cancellationToken = default)
    {
        // 阵容排名算法：使用新的阵容强度评分算法
        var lineupScores = new List<(LineupData Lineup, double Score)>();
        
        foreach (var lineup in lineups)
        {
            var strength = await _lineupAnalysisService.CalculateLineupStrengthAsync(setVersion, lineup.Id, cancellationToken);
            lineupScores.Add((lineup, strength));
        }
        
        return lineupScores
            .OrderByDescending(x => x.Score)
            .Select(x => x.Lineup);
    }
}
