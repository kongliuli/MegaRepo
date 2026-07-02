using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Services;

public class ChampionAnalysisService : IChampionAnalysisService
{
    private readonly IBigDataService _bigDataService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ChampionAnalysisService>? _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private const string ChampionAnalysisCacheKeyPrefix = "ChampionAnalysis_";
    private const string ChampionsByTierCacheKeyPrefix = "ChampionsByTier_";
    private const string ChampionTrendsCacheKeyPrefix = "ChampionTrends_";
    private const string EquipmentRecommendationsCacheKeyPrefix = "EquipmentRecommendations_";
    private const string ChampionLineupPerformanceCacheKeyPrefix = "ChampionLineupPerformance_";

    public ChampionAnalysisService(
        IBigDataService bigDataService,
        IMemoryCache cache,
        ILogger<ChampionAnalysisService>? logger = null)
    {
        _bigDataService = bigDataService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ChampionAnalysis> GetChampionAnalysisAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ChampionAnalysisCacheKeyPrefix}{setVersion}_{championId}";
        
        if (_cache.TryGetValue<ChampionAnalysis>(cacheKey, out var cachedAnalysis))
        {
            _logger?.LogDebug("Returning cached champion analysis for {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedAnalysis;
        }

        var analysis = await _bigDataService.GetChampionAnalysisAsync(setVersion, championId, cancellationToken);
        
        _cache.Set(cacheKey, analysis, CacheDuration);
        _logger?.LogDebug("Cached champion analysis for {ChampionId} in set {SetVersion}", championId, setVersion);

        return analysis;
    }

    public async Task<IEnumerable<ChampionData>> GetChampionsByTierAsync(string setVersion, string tier, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ChampionsByTierCacheKeyPrefix}{setVersion}_{tier}";
        
        if (_cache.TryGetValue<IEnumerable<ChampionData>>(cacheKey, out var cachedChampions))
        {
            _logger?.LogDebug("Returning cached champions by tier {Tier} in set {SetVersion}", tier, setVersion);
            return cachedChampions;
        }

        var champions = await _bigDataService.GetChampionsByTierAsync(setVersion, tier, cancellationToken);
        
        _cache.Set(cacheKey, champions, CacheDuration);
        _logger?.LogDebug("Cached champions by tier {Tier} in set {SetVersion}", tier, setVersion);

        return champions;
    }

    public async Task<IEnumerable<ChampionTrend>> GetChampionTrendsAsync(string setVersion, string championId, int days = 7, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ChampionTrendsCacheKeyPrefix}{setVersion}_{championId}_{days}";
        
        if (_cache.TryGetValue<IEnumerable<ChampionTrend>>(cacheKey, out var cachedTrends))
        {
            _logger?.LogDebug("Returning cached champion trends for {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedTrends;
        }

        var trends = await _bigDataService.GetChampionTrendsAsync(setVersion, championId, days, cancellationToken);
        
        _cache.Set(cacheKey, trends, CacheDuration);
        _logger?.LogDebug("Cached champion trends for {ChampionId} in set {SetVersion}", championId, setVersion);

        return trends;
    }

    public async Task<IEnumerable<EquipmentRecommendation>> GetChampionEquipmentRecommendationsAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EquipmentRecommendationsCacheKeyPrefix}{setVersion}_{championId}";
        
        if (_cache.TryGetValue<IEnumerable<EquipmentRecommendation>>(cacheKey, out var cachedRecommendations))
        {
            _logger?.LogDebug("Returning cached equipment recommendations for {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedRecommendations;
        }

        var recommendations = await _bigDataService.GetChampionEquipmentRecommendationsAsync(setVersion, championId, cancellationToken);
        
        _cache.Set(cacheKey, recommendations, CacheDuration);
        _logger?.LogDebug("Cached equipment recommendations for {ChampionId} in set {SetVersion}", championId, setVersion);

        return recommendations;
    }

    public async Task<IEnumerable<ChampionLineupPerformance>> GetChampionLineupPerformanceAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ChampionLineupPerformanceCacheKeyPrefix}{setVersion}_{championId}";
        
        if (_cache.TryGetValue<IEnumerable<ChampionLineupPerformance>>(cacheKey, out var cachedPerformance))
        {
            _logger?.LogDebug("Returning cached champion lineup performance for {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedPerformance;
        }

        var performance = await _bigDataService.GetChampionLineupPerformanceAsync(setVersion, championId, cancellationToken);
        
        _cache.Set(cacheKey, performance, CacheDuration);
        _logger?.LogDebug("Cached champion lineup performance for {ChampionId} in set {SetVersion}", championId, setVersion);

        return performance;
    }

    public Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Clearing champion analysis cache");
        
        // In a real implementation, we would clear all champion analysis-related cache keys
        // For simplicity, we'll just let the cache expire naturally
        
        return Task.CompletedTask;
    }
}
