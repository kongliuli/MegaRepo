using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Data;

public class BigDataService : IBigDataService
{
    private readonly BigDataRepository _repository;
    private readonly IBigDataCache _cache;
    private readonly IDataVersionManager _versionManager;
    private readonly IGameDataSource _externalDataSource;
    private readonly ILogger<BigDataService>? _logger;

    public BigDataService(
        BigDataRepository repository,
        IBigDataCache cache,
        IDataVersionManager versionManager,
        IGameDataSource externalDataSource,
        ILogger<BigDataService>? logger = null)
    {
        _repository = repository;
        _cache = cache;
        _versionManager = versionManager;
        _externalDataSource = externalDataSource;
        _logger = logger;
    }

    // 元数据操作
    public async Task<MetaData?> GetMetaDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cachedMetaData = await _cache.GetMetaDataAsync(setVersion, cancellationToken);
        if (cachedMetaData != null)
        {
            _logger?.LogDebug("MetaData cache hit for set {SetVersion}", setVersion);
            return cachedMetaData;
        }

        // 缓存未命中，从数据库获取
        var metaData = await _repository.GetMetaDataAsync(setVersion, cancellationToken);
        if (metaData != null)
        {
            // 存入缓存
            await _cache.SetMetaDataAsync(setVersion, metaData, cancellationToken);
            _logger?.LogDebug("MetaData cache miss, loaded from database for set {SetVersion}", setVersion);
            return metaData;
        }

        // 数据库未命中，从外部API获取
        _logger?.LogDebug("MetaData not found in database, fetching from external API for set {SetVersion}", setVersion);
        metaData = await _externalDataSource.GetMetaDataAsync(setVersion, cancellationToken);
        if (metaData != null)
        {
            // 保存到数据库
            await _repository.SaveMetaDataAsync(metaData, cancellationToken);
            // 存入缓存
            await _cache.SetMetaDataAsync(setVersion, metaData, cancellationToken);
            // 管理版本
            await _versionManager.CreateOrUpdateVersionAsync(metaData, cancellationToken);
            _logger?.LogInformation("MetaData fetched from external API and saved for set {SetVersion}", setVersion);
        }

        return metaData;
    }

    public async Task SaveMetaDataAsync(MetaData metaData, CancellationToken cancellationToken = default)
    {
        // 保存到数据库
        await _repository.SaveMetaDataAsync(metaData, cancellationToken);
        // 更新缓存
        await _cache.SetMetaDataAsync(metaData.SetVersion, metaData, cancellationToken);
        // 管理版本
        await _versionManager.CreateOrUpdateVersionAsync(metaData, cancellationToken);
        _logger?.LogInformation("Saved MetaData for set {SetVersion}, version {Version}", metaData.SetVersion, metaData.Version);
    }

    // 阵容数据操作
    public async Task<IEnumerable<LineupData>> GetLineupDataAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cachedLineups = await _cache.GetLineupDataAsync(setVersion, cancellationToken);
        if (cachedLineups != null)
        {
            _logger?.LogDebug("LineupData cache hit for set {SetVersion}", setVersion);
            return cachedLineups;
        }

        // 缓存未命中，从数据库获取
        var lineups = await _repository.GetLineupDataAsync(setVersion, cancellationToken);
        if (lineups.Any())
        {
            // 存入缓存
            await _cache.SetLineupDataAsync(setVersion, lineups, cancellationToken);
            _logger?.LogDebug("LineupData cache miss, loaded {Count} items from database for set {SetVersion}", lineups.Count(), setVersion);
            return lineups;
        }

        // 数据库未命中，从外部API获取
        _logger?.LogDebug("LineupData not found in database, fetching from external API for set {SetVersion}", setVersion);
        
        // 使用超时和重试机制
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10)); // 10秒超时
        
        try
        {
            lineups = await _externalDataSource.GetLineupDataAsync(setVersion, cts.Token);
            if (lineups.Any())
            {
                // 批量保存到数据库，使用并行处理
                await _repository.SaveLineupDataBatchAsync(lineups, cancellationToken);
                // 存入缓存
                await _cache.SetLineupDataAsync(setVersion, lineups, cancellationToken);
                _logger?.LogInformation("LineupData fetched from external API and saved for set {SetVersion}", setVersion);
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("LineupData fetch from external API timed out for set {SetVersion}", setVersion);
            lineups = Enumerable.Empty<LineupData>();
        }

        return lineups;
    }

    public async Task<IEnumerable<LineupData>> GetLineupsAsync(string setVersion, string tier = "all", CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"lineups:{setVersion}:{tier}";
        var cachedLineups = await _cache.GetAsync<IEnumerable<LineupData>>(cacheKey, cancellationToken);
        if (cachedLineups != null)
        {
            _logger?.LogDebug("Lineups cache hit for set {SetVersion} and tier {Tier}", setVersion, tier);
            return cachedLineups;
        }

        // 缓存未命中，从数据库获取
        // 这里使用模拟数据，实际应该从数据库获取对应段位的数据
        var allLineups = await GetLineupDataAsync(setVersion, cancellationToken);
        var filteredLineups = allLineups;

        // 根据段位过滤
        if (tier != "all")
        {
            // 模拟段位过滤逻辑
            filteredLineups = allLineups.Where(l => l.MatchCount > 100); // 简单模拟，实际应该根据段位数据过滤
        }

        // 存入缓存
        await _cache.SetAsync(cacheKey, filteredLineups, cancellationToken);
        _logger?.LogDebug("Lineups cache miss, loaded {Count} items from database for set {SetVersion} and tier {Tier}", filteredLineups.Count(), setVersion, tier);

        return filteredLineups;
    }

    public async Task SaveLineupDataAsync(LineupData lineupData, CancellationToken cancellationToken = default)
    {
        // 保存到数据库
        await _repository.SaveLineupDataAsync(lineupData, cancellationToken);
        // 清除相关缓存
        await _cache.RemoveAsync($"lineups:{lineupData.SetVersion}", cancellationToken);
        _logger?.LogDebug("Saved LineupData with Id: {Id}", lineupData.Id);
    }

    public async Task SaveLineupDataBatchAsync(IEnumerable<LineupData> lineups, CancellationToken cancellationToken = default)
    {
        if (!lineups.Any()) return;

        var setVersion = lineups.First().SetVersion;
        // 批量保存到数据库
        await _repository.SaveLineupDataBatchAsync(lineups, cancellationToken);
        // 清除相关缓存
        await _cache.RemoveAsync($"lineups:{setVersion}", cancellationToken);
        _logger?.LogInformation("Bulk saved {Count} lineups for set {SetVersion}", lineups.Count(), setVersion);
    }

    // 装备数据操作
    public async Task<IEnumerable<EquipmentData>> GetEquipmentDataAsync(string setVersion, bool? isComponent = null, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cachedEquipment = await _cache.GetEquipmentDataAsync(setVersion, isComponent, cancellationToken);
        if (cachedEquipment != null)
        {
            _logger?.LogDebug("EquipmentData cache hit for set {SetVersion}, component: {IsComponent}", setVersion, isComponent);
            return cachedEquipment;
        }

        // 缓存未命中，从数据库获取
        var equipmentList = await _repository.GetEquipmentDataAsync(setVersion, isComponent, cancellationToken);
        if (equipmentList.Any())
        {
            // 如果是获取所有装备，同时缓存按组件类型分类的数据
            if (!isComponent.HasValue)
            {
                await _cache.SetEquipmentDataAsync(setVersion, equipmentList, cancellationToken);
            }
            _logger?.LogDebug("EquipmentData cache miss, loaded {Count} items from database for set {SetVersion}", equipmentList.Count(), setVersion);
            return equipmentList;
        }

        // 数据库未命中，从外部API获取
        _logger?.LogDebug("EquipmentData not found in database, fetching from external API for set {SetVersion}", setVersion);
        
        // 使用超时和重试机制
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(8)); // 8秒超时
        
        try
        {
            equipmentList = await _externalDataSource.GetEquipmentDataAsync(setVersion, isComponent, cts.Token);
            if (equipmentList.Any())
            {
                // 批量保存到数据库
                await _repository.SaveEquipmentDataBatchAsync(equipmentList, cancellationToken);
                // 如果是获取所有装备，同时缓存按组件类型分类的数据
                if (!isComponent.HasValue)
                {
                    await _cache.SetEquipmentDataAsync(setVersion, equipmentList, cancellationToken);
                }
                _logger?.LogInformation("EquipmentData fetched from external API and saved for set {SetVersion}", setVersion);
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("EquipmentData fetch from external API timed out for set {SetVersion}", setVersion);
            equipmentList = Enumerable.Empty<EquipmentData>();
        }

        return equipmentList;
    }

    public async Task SaveEquipmentDataAsync(EquipmentData equipmentData, CancellationToken cancellationToken = default)
    {
        // 保存到数据库
        await _repository.SaveEquipmentDataAsync(equipmentData, cancellationToken);
        // 清除相关缓存
        await _cache.RemoveAsync($"equipment:{equipmentData.SetVersion}:null", cancellationToken);
        await _cache.RemoveAsync($"equipment:{equipmentData.SetVersion}:{equipmentData.IsComponent}", cancellationToken);
        _logger?.LogDebug("Saved EquipmentData with Id: {Id}", equipmentData.Id);
    }

    public async Task SaveEquipmentDataBatchAsync(IEnumerable<EquipmentData> equipmentList, CancellationToken cancellationToken = default)
    {
        if (!equipmentList.Any()) return;

        var setVersion = equipmentList.First().SetVersion;
        // 批量保存到数据库
        await _repository.SaveEquipmentDataBatchAsync(equipmentList, cancellationToken);
        // 清除相关缓存
        await _cache.RemoveAsync($"equipment:{setVersion}:null", cancellationToken);
        await _cache.RemoveAsync($"equipment:{setVersion}:True", cancellationToken);
        await _cache.RemoveAsync($"equipment:{setVersion}:False", cancellationToken);
        _logger?.LogInformation("Bulk saved {Count} equipment items for set {SetVersion}", equipmentList.Count(), setVersion);
    }

    // 英雄数据操作
    public async Task<IEnumerable<ChampionData>> GetChampionDataAsync(string setVersion, int? cost = null, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cachedChampions = await _cache.GetChampionDataAsync(setVersion, cost, cancellationToken);
        if (cachedChampions != null)
        {
            _logger?.LogDebug("ChampionData cache hit for set {SetVersion}, cost: {Cost}", setVersion, cost);
            return cachedChampions;
        }

        // 缓存未命中，从数据库获取
        var champions = await _repository.GetChampionDataAsync(setVersion, cost, cancellationToken);
        if (champions.Any())
        {
            // 如果是获取所有英雄，同时缓存按费用分类的数据
            if (!cost.HasValue)
            {
                await _cache.SetChampionDataAsync(setVersion, champions, cancellationToken);
            }
            _logger?.LogDebug("ChampionData cache miss, loaded {Count} items from database for set {SetVersion}", champions.Count(), setVersion);
            return champions;
        }

        // 数据库未命中，从外部API获取
        _logger?.LogDebug("ChampionData not found in database, fetching from external API for set {SetVersion}", setVersion);
        
        // 使用超时和重试机制
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(8)); // 8秒超时
        
        try
        {
            champions = await _externalDataSource.GetChampionDataAsync(setVersion, cost, cts.Token);
            if (champions.Any())
            {
                // 批量保存到数据库
                await _repository.SaveChampionDataBatchAsync(champions, cancellationToken);
                // 如果是获取所有英雄，同时缓存按费用分类的数据
                if (!cost.HasValue)
                {
                    await _cache.SetChampionDataAsync(setVersion, champions, cancellationToken);
                }
                _logger?.LogInformation("ChampionData fetched from external API and saved for set {SetVersion}", setVersion);
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("ChampionData fetch from external API timed out for set {SetVersion}", setVersion);
            champions = Enumerable.Empty<ChampionData>();
        }

        return champions;
    }

    public async Task<ChampionData?> GetChampionDataByIdAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"champion:{setVersion}:{championId}";
        var cachedChampion = await _cache.GetAsync<ChampionData>(cacheKey, cancellationToken);
        if (cachedChampion != null)
        {
            _logger?.LogDebug("ChampionData cache hit for champion {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedChampion;
        }

        // 缓存未命中，从数据库获取
        // 实际应该从数据库获取，这里使用模拟数据
        var champion = GenerateMockChampionData(setVersion, championId);
        
        // 存入缓存
        if (champion != null)
        {
            await _cache.SetAsync(cacheKey, champion, cancellationToken);
            _logger?.LogDebug("ChampionData cache miss, generated data for champion {ChampionId}", championId);
        }

        return champion;
    }

    public async Task<IEnumerable<ChampionData>> GetChampionsByTierAsync(string setVersion, string tier, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"champions_by_tier:{setVersion}:{tier}";
        var cachedChampions = await _cache.GetAsync<IEnumerable<ChampionData>>(cacheKey, cancellationToken);
        if (cachedChampions != null)
        {
            _logger?.LogDebug("ChampionsByTier cache hit for tier {Tier} in set {SetVersion}", tier, setVersion);
            return cachedChampions;
        }

        // 缓存未命中，从数据库获取
        // 实际应该从数据库获取对应段位的英雄数据，这里使用模拟数据
        var champions = GenerateMockChampionsByTier(setVersion, tier);
        
        // 存入缓存
        await _cache.SetAsync(cacheKey, champions, cancellationToken);
        _logger?.LogDebug("ChampionsByTier cache miss, generated {Count} champions for tier {Tier}", champions.Count(), tier);

        return champions;
    }

    public async Task<IEnumerable<ChampionTrend>> GetChampionTrendsAsync(string setVersion, string championId, int days = 7, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"champion_trends:{setVersion}:{championId}:{days}";
        var cachedTrends = await _cache.GetAsync<IEnumerable<ChampionTrend>>(cacheKey, cancellationToken);
        if (cachedTrends != null)
        {
            _logger?.LogDebug("ChampionTrends cache hit for champion {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedTrends;
        }

        // 缓存未命中，生成模拟数据
        var trends = GenerateMockChampionTrends(championId, days);
        
        // 存入缓存
        await _cache.SetAsync(cacheKey, trends, cancellationToken);
        _logger?.LogDebug("ChampionTrends cache miss, generated {Count} days of trends for champion {ChampionId}", trends.Count(), championId);

        return trends;
    }

    public async Task<ChampionAnalysis> GetChampionAnalysisAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"champion_analysis:{setVersion}:{championId}";
        var cachedAnalysis = await _cache.GetAsync<ChampionAnalysis>(cacheKey, cancellationToken);
        if (cachedAnalysis != null)
        {
            _logger?.LogDebug("ChampionAnalysis cache hit for champion {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedAnalysis;
        }

        // 缓存未命中，从外部API获取
        _logger?.LogDebug("ChampionAnalysis not found in cache, fetching from external API for champion {ChampionId}", championId);
        var analysis = await _externalDataSource.GetChampionAnalysisAsync(setVersion, championId, cancellationToken);
        
        if (analysis == null)
        {
            // 外部API获取失败，生成模拟数据
            analysis = GenerateMockChampionAnalysis(setVersion, championId);
            _logger?.LogWarning("Failed to fetch ChampionAnalysis from external API, using mock data for champion {ChampionId}", championId);
        }
        else
        {
            _logger?.LogInformation("ChampionAnalysis fetched from external API for champion {ChampionId}", championId);
        }
        
        // 存入缓存
        await _cache.SetAsync(cacheKey, analysis, cancellationToken);

        return analysis;
    }

    public async Task<IEnumerable<EquipmentRecommendation>> GetChampionEquipmentRecommendationsAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"equipment_recommendations:{setVersion}:{championId}";
        var cachedRecommendations = await _cache.GetAsync<IEnumerable<EquipmentRecommendation>>(cacheKey, cancellationToken);
        if (cachedRecommendations != null)
        {
            _logger?.LogDebug("EquipmentRecommendations cache hit for champion {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedRecommendations;
        }

        // 缓存未命中，从外部API获取
        _logger?.LogDebug("EquipmentRecommendations not found in cache, fetching from external API for champion {ChampionId}", championId);
        var recommendations = await _externalDataSource.GetEquipmentRecommendationsAsync(setVersion, championId, cancellationToken);
        
        if (!recommendations.Any())
        {
            // 外部API获取失败，生成模拟数据
            recommendations = GenerateMockEquipmentRecommendations(championId);
            _logger?.LogWarning("Failed to fetch EquipmentRecommendations from external API, using mock data for champion {ChampionId}", championId);
        }
        else
        {
            _logger?.LogInformation("EquipmentRecommendations fetched from external API for champion {ChampionId}", championId);
        }
        
        // 存入缓存
        await _cache.SetAsync(cacheKey, recommendations, cancellationToken);

        return recommendations;
    }

    public async Task<IEnumerable<ChampionLineupPerformance>> GetChampionLineupPerformanceAsync(string setVersion, string championId, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"champion_lineup_performance:{setVersion}:{championId}";
        var cachedPerformance = await _cache.GetAsync<IEnumerable<ChampionLineupPerformance>>(cacheKey, cancellationToken);
        if (cachedPerformance != null)
        {
            _logger?.LogDebug("ChampionLineupPerformance cache hit for champion {ChampionId} in set {SetVersion}", championId, setVersion);
            return cachedPerformance;
        }

        // 缓存未命中，生成模拟数据
        var performance = GenerateMockChampionLineupPerformance(championId);
        
        // 存入缓存
        await _cache.SetAsync(cacheKey, performance, cancellationToken);
        _logger?.LogDebug("ChampionLineupPerformance cache miss, generated {Count} lineup performances for champion {ChampionId}", performance.Count(), championId);

        return performance;
    }

    public async Task SaveChampionDataAsync(ChampionData championData, CancellationToken cancellationToken = default)
    {
        // 保存到数据库
        await _repository.SaveChampionDataAsync(championData, cancellationToken);
        // 清除相关缓存
        await _cache.RemoveAsync($"champions:{championData.SetVersion}:null", cancellationToken);
        await _cache.RemoveAsync($"champions:{championData.SetVersion}:{championData.Cost}", cancellationToken);
        await _cache.RemoveAsync($"champion:{championData.SetVersion}:{championData.Id}", cancellationToken);
        _logger?.LogDebug("Saved ChampionData with Id: {Id}", championData.Id);
    }

    public async Task SaveChampionDataBatchAsync(IEnumerable<ChampionData> champions, CancellationToken cancellationToken = default)
    {
        if (!champions.Any()) return;

        var setVersion = champions.First().SetVersion;
        // 批量保存到数据库
        await _repository.SaveChampionDataBatchAsync(champions, cancellationToken);
        // 清除相关缓存
        await _cache.RemoveAsync($"champions:{setVersion}:null", cancellationToken);
        // 清除按费用分类的缓存
        var costs = champions.Select(c => c.Cost).Distinct();
        foreach (var cost in costs)
        {
            await _cache.RemoveAsync($"champions:{setVersion}:{cost}", cancellationToken);
        }
        // 清除单个英雄缓存
        foreach (var champion in champions)
        {
            await _cache.RemoveAsync($"champion:{setVersion}:{champion.Id}", cancellationToken);
        }
        _logger?.LogInformation("Bulk saved {Count} champions for set {SetVersion}", champions.Count(), setVersion);
    }

    // 缓存管理
    public async Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        await _cache.ClearAsync(cancellationToken);
        _logger?.LogInformation("Cache cleared");
    }

    public async Task<int> GetCacheSizeAsync(CancellationToken cancellationToken = default)
    {
        var size = await _cache.GetCacheSizeAsync(cancellationToken);
        _logger?.LogDebug("Current cache size: {Size}", size);
        return size;
    }

    // 版本管理
    public async Task<string?> GetLatestVersionAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        return await _versionManager.GetLatestVersionAsync(setVersion, cancellationToken);
    }

    public async Task<ConsistencyCheckResult> CheckDataConsistencyAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        return await _versionManager.PerformConsistencyCheckAsync(setVersion, cancellationToken);
    }

    // Meta 分析操作
    public async Task<IEnumerable<LineupTrend>> GetLineupTrendsAsync(string setVersion, string lineupId, int days = 7, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"lineup_trends:{setVersion}:{lineupId}:{days}";
        var cachedTrends = await _cache.GetAsync<IEnumerable<LineupTrend>>(cacheKey, cancellationToken);
        if (cachedTrends != null)
        {
            _logger?.LogDebug("LineupTrends cache hit for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);
            return cachedTrends;
        }

        // 缓存未命中，生成模拟数据
        // 实际应该从数据库获取历史趋势数据
        var trends = GenerateMockLineupTrends(lineupId, days);

        // 存入缓存
        await _cache.SetAsync(cacheKey, trends, cancellationToken);
        _logger?.LogDebug("LineupTrends cache miss, generated {Count} days of trends for lineup {LineupId}", trends.Count(), lineupId);

        return trends;
    }

    public async Task<MetaTierAnalysis> GetMetaTierAnalysisAsync(string setVersion, CancellationToken cancellationToken = default)
    {
        // 先从缓存获取
        var cacheKey = $"meta_tier_analysis:{setVersion}";
        var cachedAnalysis = await _cache.GetAsync<MetaTierAnalysis>(cacheKey, cancellationToken);
        if (cachedAnalysis != null)
        {
            _logger?.LogDebug("MetaTierAnalysis cache hit for set {SetVersion}", setVersion);
            return cachedAnalysis;
        }

        // 缓存未命中，从外部API获取
        _logger?.LogDebug("MetaTierAnalysis not found in cache, fetching from external API for set {SetVersion}", setVersion);
        var analysis = await _externalDataSource.GetMetaTierAnalysisAsync(setVersion, cancellationToken);
        
        if (analysis == null)
        {
            // 外部API获取失败，生成模拟数据
            analysis = GenerateMockMetaTierAnalysis(setVersion);
            _logger?.LogWarning("Failed to fetch MetaTierAnalysis from external API, using mock data for set {SetVersion}", setVersion);
        }
        else
        {
            _logger?.LogInformation("MetaTierAnalysis fetched from external API for set {SetVersion}", setVersion);
        }

        // 存入缓存
        await _cache.SetAsync(cacheKey, analysis, cancellationToken);

        return analysis;
    }

    // 生成模拟阵容趋势数据
    private IEnumerable<LineupTrend> GenerateMockLineupTrends(string lineupId, int days)
    {
        var trends = new List<LineupTrend>();
        var random = new Random();
        var baseWinRate = 0.45 + random.NextDouble() * 0.2; // 45% - 65%
        var basePickRate = 0.05 + random.NextDouble() * 0.15; // 5% - 20%
        var basePlacement = 3 + random.Next(3); // 3 - 5

        for (int i = days - 1; i >= 0; i--)
        {
            var date = DateTime.Now.AddDays(-i);
            var winRateVariation = (random.NextDouble() - 0.5) * 0.05; // ±5%
            var pickRateVariation = (random.NextDouble() - 0.5) * 0.02; // ±2%
            var placementVariation = random.Next(-1, 2); // ±1

            trends.Add(new LineupTrend
            {
                LineupId = lineupId,
                LineupName = "模拟阵容" + random.Next(1000),
                Date = date,
                WinRate = Math.Max(0.1, Math.Min(0.8, baseWinRate + winRateVariation)),
                PickRate = Math.Max(0.01, Math.Min(0.3, basePickRate + pickRateVariation)),
                MatchCount = 1000 + random.Next(1000),
                AveragePlacement = Math.Max(1, Math.Min(8, basePlacement + placementVariation))
            });
        }

        return trends;
    }

    // 生成模拟 Meta 段位分析数据
    private MetaTierAnalysis GenerateMockMetaTierAnalysis(string setVersion)
    {
        var tiers = new[] { "iron", "bronze", "silver", "gold", "platinum", "diamond", "master", "challenger" };
        var tierLineups = new Dictionary<string, IEnumerable<LineupData>>();
        var tierStats = new Dictionary<string, MetaTierStats>();
        var random = new Random();

        foreach (var tier in tiers)
        {
            // 为每个段位生成模拟阵容数据
            var lineups = new List<LineupData>();
            for (int i = 0; i < 10; i++)
            {
                lineups.Add(new LineupData
                {
                    Id = $"{tier}_lineup_{i}",
                    Name = $"{tier} 阵容 {i}",
                    SetVersion = setVersion,
                    Champions = Enumerable.Range(1, 5).Select(j => $"champion_{j}").ToList(),
                    Traits = Enumerable.Range(1, 3).Select(j => $"trait_{j}").ToList(),
                    ChampionItems = new List<ChampionItem>(),
                    WinRate = 0.4 + random.NextDouble() * 0.3, // 40% - 70%
                    PickRate = 0.03 + random.NextDouble() * 0.12, // 3% - 15%
                    MatchCount = 500 + random.Next(1500),
                    AveragePlacement = 2 + random.Next(4), // 2 - 5
                    LastUpdated = DateTime.Now
                });
            }

            tierLineups[tier] = lineups;
            tierStats[tier] = new MetaTierStats
            {
                Tier = tier,
                TotalMatches = 10000 + random.Next(50000),
                TotalPlayers = 1000 + random.Next(5000),
                AverageGameTime = 25 + random.Next(10) // 25 - 35 minutes
            };
        }

        return new MetaTierAnalysis
        {
            SetVersion = setVersion,
            LastUpdated = DateTime.Now,
            TierLineups = tierLineups,
            TierStats = tierStats
        };
    }

    // 生成模拟英雄数据
    private ChampionData? GenerateMockChampionData(string setVersion, string championId)
    {
        var random = new Random();
        var tiers = new[] { "iron", "bronze", "silver", "gold", "platinum", "diamond", "master", "challenger" };
        var winRateByTier = new Dictionary<string, double>();
        var pickRateByTier = new Dictionary<string, double>();

        foreach (var tier in tiers)
        {
            winRateByTier[tier] = 0.4 + random.NextDouble() * 0.3; // 40% - 70%
            pickRateByTier[tier] = 0.02 + random.NextDouble() * 0.15; // 2% - 17%
        }

        return new ChampionData
        {
            Id = championId,
            Name = $"英雄 {championId.Replace("champion_", "")}",
            SetVersion = setVersion,
            Cost = 1 + random.Next(5), // 1-5
            Traits = Enumerable.Range(1, 2).Select(j => $"trait_{j}").ToList(),
            ImageUrl = $"https://example.com/champions/{championId}.png",
            WinRate = 0.45 + random.NextDouble() * 0.2, // 45% - 65%
            PickRate = 0.05 + random.NextDouble() * 0.15, // 5% - 20%
            AveragePlacement = 2.5 + random.NextDouble() * 2, // 2.5 - 4.5
            MatchCount = 1000 + random.Next(9000), // 1000 - 10000
            LastUpdated = DateTime.Now,
            IsHistoricalData = true,
            WinRateByTier = winRateByTier,
            PickRateByTier = pickRateByTier,
            Trends = GenerateMockChampionTrends(championId, 7).ToList()
        };
    }

    // 生成按段位的模拟英雄数据
    private IEnumerable<ChampionData> GenerateMockChampionsByTier(string setVersion, string tier)
    {
        var champions = new List<ChampionData>();
        var random = new Random();

        for (int i = 0; i < 15; i++)
        {
            var championId = $"champion_{i}";
            champions.Add(new ChampionData
            {
                Id = championId,
                Name = $"英雄 {i}",
                SetVersion = setVersion,
                Cost = 1 + random.Next(5), // 1-5
                Traits = Enumerable.Range(1, 2).Select(j => $"trait_{j}").ToList(),
                ImageUrl = $"https://example.com/champions/{championId}.png",
                WinRate = 0.4 + random.NextDouble() * 0.3, // 40% - 70%
                PickRate = 0.03 + random.NextDouble() * 0.12, // 3% - 15%
                AveragePlacement = 2 + random.NextDouble() * 3, // 2 - 5
                MatchCount = 500 + random.Next(1500), // 500 - 2000
                LastUpdated = DateTime.Now,
                IsHistoricalData = true
            });
        }

        // 按胜率排序
        return champions.OrderByDescending(c => c.WinRate);
    }

    // 生成模拟英雄趋势数据
    private IEnumerable<ChampionTrend> GenerateMockChampionTrends(string championId, int days)
    {
        var trends = new List<ChampionTrend>();
        var random = new Random();
        var baseWinRate = 0.45 + random.NextDouble() * 0.2; // 45% - 65%
        var basePickRate = 0.05 + random.NextDouble() * 0.15; // 5% - 20%
        var basePlacement = 3 + random.NextDouble() * 1; // 3 - 4

        for (int i = days - 1; i >= 0; i--)
        {
            var date = DateTime.Now.AddDays(-i);
            var winRateVariation = (random.NextDouble() - 0.5) * 0.05; // ±5%
            var pickRateVariation = (random.NextDouble() - 0.5) * 0.02; // ±2%
            var placementVariation = (random.NextDouble() - 0.5) * 0.5; // ±0.25

            trends.Add(new ChampionTrend
            {
                Date = date,
                WinRate = Math.Max(0.1, Math.Min(0.8, baseWinRate + winRateVariation)),
                PickRate = Math.Max(0.01, Math.Min(0.3, basePickRate + pickRateVariation)),
                MatchCount = 800 + random.Next(400), // 800 - 1200
                AveragePlacement = Math.Max(1, Math.Min(8, basePlacement + placementVariation))
            });
        }

        return trends;
    }

    // 生成模拟英雄分析数据
    private ChampionAnalysis GenerateMockChampionAnalysis(string setVersion, string championId)
    {
        var random = new Random();
        var tiers = new[] { "iron", "bronze", "silver", "gold", "platinum", "diamond", "master", "challenger" };
        var winRateByTier = new Dictionary<string, double>();
        var pickRateByTier = new Dictionary<string, double>();

        foreach (var tier in tiers)
        {
            winRateByTier[tier] = 0.4 + random.NextDouble() * 0.3; // 40% - 70%
            pickRateByTier[tier] = 0.02 + random.NextDouble() * 0.15; // 2% - 17%
        }

        return new ChampionAnalysis
        {
            ChampionId = championId,
            ChampionName = $"英雄 {championId.Replace("champion_", "")}",
            SetVersion = setVersion,
            OverallWinRate = 0.45 + random.NextDouble() * 0.2, // 45% - 65%
            OverallPickRate = 0.05 + random.NextDouble() * 0.15, // 5% - 20%
            AveragePlacement = 2.5 + random.NextDouble() * 2, // 2.5 - 4.5
            MatchCount = 5000 + random.Next(15000), // 5000 - 20000
            WinRateByTier = winRateByTier,
            PickRateByTier = pickRateByTier,
            Trends = GenerateMockChampionTrends(championId, 7).ToList(),
            LastUpdated = DateTime.Now,
            IsHistoricalData = true
        };
    }

    // 生成模拟装备推荐数据
    private IEnumerable<EquipmentRecommendation> GenerateMockEquipmentRecommendations(string championId)
    {
        var recommendations = new List<EquipmentRecommendation>();
        var equipmentNames = new[] { "狂徒铠甲", "日炎斗篷", "荆棘之甲", "守护天使", "正义之手", "巨人杀手", "无尽之刃", "珠光护手", "饮血剑", "水银" };
        var random = new Random();

        for (int i = 0; i < 6; i++)
        {
            recommendations.Add(new EquipmentRecommendation
            {
                EquipmentId = $"equipment_{i}",
                EquipmentName = equipmentNames[i],
                WinRate = 0.5 + random.NextDouble() * 0.2, // 50% - 70%
                PickRate = 0.1 + random.NextDouble() * 0.3, // 10% - 40%
                MatchCount = 1000 + random.Next(4000), // 1000 - 5000
                Priority = 6 - i // 1-6
            });
        }

        return recommendations;
    }

    // 生成模拟英雄阵容表现数据
    private IEnumerable<ChampionLineupPerformance> GenerateMockChampionLineupPerformance(string championId)
    {
        var performances = new List<ChampionLineupPerformance>();
        var random = new Random();

        for (int i = 0; i < 8; i++)
        {
            performances.Add(new ChampionLineupPerformance
            {
                LineupId = $"lineup_{i}",
                LineupName = $"阵容 {i}",
                WinRate = 0.4 + random.NextDouble() * 0.3, // 40% - 70%
                PickRate = 0.03 + random.NextDouble() * 0.12, // 3% - 15%
                MatchCount = 500 + random.Next(1500), // 500 - 2000
                AveragePlacement = 2 + random.NextDouble() * 3, // 2 - 5
                SynergyScore = 60 + random.Next(40) // 60 - 100
            });
        }

        // 按胜率排序
        return performances.OrderByDescending(p => p.WinRate);
    }
}
