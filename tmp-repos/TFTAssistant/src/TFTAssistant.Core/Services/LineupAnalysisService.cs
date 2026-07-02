using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.BigData;

namespace TFTAssistant.Core.Services;

public class LineupAnalysisService : ILineupAnalysisService
{
    private readonly IBigDataService _bigDataService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LineupAnalysisService>? _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private const string LineupStrengthCacheKeyPrefix = "LineupStrength_";
    private const string EquipmentPriorityCacheKeyPrefix = "EquipmentPriority_";
    private const string CounterAnalysisCacheKeyPrefix = "CounterAnalysis_";

    public LineupAnalysisService(
        IBigDataService bigDataService,
        IMemoryCache cache,
        ILogger<LineupAnalysisService>? logger = null)
    {
        _bigDataService = bigDataService;
        _cache = cache;
        _logger = logger;
    }

    // 1. 阵容强度评分算法
    public async Task<double> CalculateLineupStrengthAsync(string setVersion, string lineupId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{LineupStrengthCacheKeyPrefix}{setVersion}_{lineupId}";
        
        if (_cache.TryGetValue<double>(cacheKey, out var cachedStrength))
        {
            _logger?.LogDebug("Returning cached lineup strength for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);
            return cachedStrength;
        }

        var lineups = await _bigDataService.GetLineupDataAsync(setVersion, cancellationToken);
        var targetLineup = lineups.FirstOrDefault(l => l.Id == lineupId);
        
        if (targetLineup == null)
        {
            _logger?.LogWarning("Lineup {LineupId} not found in set {SetVersion}", lineupId, setVersion);
            return 0.0;
        }

        var strength = CalculateLineupStrengthInternal(targetLineup, lineups);
        
        _cache.Set(cacheKey, strength, CacheDuration);
        _logger?.LogDebug("Cached lineup strength for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);

        return strength;
    }

    // 2. 装备优先级算法
    public async Task<IEnumerable<EquipmentPriority>> GetEquipmentPrioritiesAsync(string setVersion, string lineupId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{EquipmentPriorityCacheKeyPrefix}{setVersion}_{lineupId}";
        
        if (_cache.TryGetValue<IEnumerable<EquipmentPriority>>(cacheKey, out var cachedPriorities))
        {
            _logger?.LogDebug("Returning cached equipment priorities for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);
            return cachedPriorities;
        }

        var lineups = await _bigDataService.GetLineupDataAsync(setVersion, cancellationToken);
        var equipmentData = await _bigDataService.GetEquipmentDataAsync(setVersion, cancellationToken);
        
        var targetLineup = lineups.FirstOrDefault(l => l.Id == lineupId);
        if (targetLineup == null)
        {
            _logger?.LogWarning("Lineup {LineupId} not found in set {SetVersion}", lineupId, setVersion);
            return Enumerable.Empty<EquipmentPriority>();
        }

        var priorities = CalculateEquipmentPriorities(targetLineup, equipmentData);
        
        _cache.Set(cacheKey, priorities, CacheDuration);
        _logger?.LogDebug("Cached equipment priorities for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);

        return priorities;
    }

    // 3. 阵容克制关系分析
    public async Task<CounterAnalysis> GetCounterAnalysisAsync(string setVersion, string lineupId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CounterAnalysisCacheKeyPrefix}{setVersion}_{lineupId}";
        
        if (_cache.TryGetValue<CounterAnalysis>(cacheKey, out var cachedAnalysis))
        {
            _logger?.LogDebug("Returning cached counter analysis for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);
            return cachedAnalysis;
        }

        var lineups = await _bigDataService.GetLineupDataAsync(setVersion, cancellationToken);
        var targetLineup = lineups.FirstOrDefault(l => l.Id == lineupId);
        
        if (targetLineup == null)
        {
            _logger?.LogWarning("Lineup {LineupId} not found in set {SetVersion}", lineupId, setVersion);
            return new CounterAnalysis { LineupId = lineupId, Counters = Enumerable.Empty<LineupCounter>(), CounteredBy = Enumerable.Empty<LineupCounter>() };
        }

        var analysis = CalculateCounterAnalysis(targetLineup, lineups);
        
        _cache.Set(cacheKey, analysis, CacheDuration);
        _logger?.LogDebug("Cached counter analysis for lineup {LineupId} in set {SetVersion}", lineupId, setVersion);

        return analysis;
    }

    // 内部方法：计算阵容强度
    private double CalculateLineupStrengthInternal(LineupData targetLineup, IEnumerable<LineupData> allLineups)
    {
        // 基础评分：基于胜率、登场率和平均排名
        var winRateScore = targetLineup.WinRate * 0.4;
        var pickRateScore = targetLineup.PickRate * 0.2;
        var placementScore = (8 - targetLineup.AveragePlacement) / 7.0 * 0.2;
        
        // 阵容多样性评分：基于英雄和特性的多样性
        var diversityScore = CalculateDiversityScore(targetLineup) * 0.1;
        
        // 装备搭配评分：基于装备的质量和搭配合理性
        var equipmentScore = CalculateEquipmentScore(targetLineup) * 0.1;
        
        // 综合评分
        var totalScore = winRateScore + pickRateScore + placementScore + diversityScore + equipmentScore;
        
        // 归一化到 0-100 分
        return Math.Min(100, totalScore * 100);
    }

    // 计算阵容多样性评分
    private double CalculateDiversityScore(LineupData lineup)
    {
        // 基于英雄数量和特性数量的多样性评分
        var championDiversity = lineup.Champions.Count / 10.0; // 假设最多10个英雄
        var traitDiversity = lineup.Traits.Count / 8.0; // 假设最多8个特性
        
        return (championDiversity + traitDiversity) / 2.0;
    }

    // 计算装备搭配评分
    private double CalculateEquipmentScore(LineupData lineup)
    {
        // 基于装备数量和分布的评分
        var totalItems = lineup.ChampionItems.Sum(ci => ci.Items.Count);
        var equippedChampions = lineup.ChampionItems.Count(ci => ci.Items.Count > 0);
        
        // 理想情况下，每个英雄都有装备，且装备数量合理
        var itemDistributionScore = Math.Min(1.0, totalItems / 12.0); // 假设最多12个装备
        var equippedChampionsScore = Math.Min(1.0, equippedChampions / lineup.Champions.Count);
        
        return (itemDistributionScore + equippedChampionsScore) / 2.0;
    }

    // 计算装备优先级
    private IEnumerable<EquipmentPriority> CalculateEquipmentPriorities(LineupData lineup, IEnumerable<EquipmentData> equipmentData)
    {
        var priorities = new List<EquipmentPriority>();
        
        foreach (var equipment in equipmentData.Where(e => !e.IsComponent))
        {
            // 基础优先级：基于装备的胜率和登场率
            var basePriority = equipment.WinRate * 0.6 + equipment.PickRate * 0.4;
            
            // 阵容适配性评分：基于装备与阵容的匹配程度
            var matchupScore = CalculateEquipmentMatchupScore(equipment, lineup);
            
            // 综合优先级
            var totalPriority = basePriority * 0.7 + matchupScore * 0.3;
            
            priorities.Add(new EquipmentPriority
            {
                EquipmentId = equipment.Id,
                EquipmentName = equipment.Name,
                PriorityScore = totalPriority,
                WinRate = equipment.WinRate,
                PickRate = equipment.PickRate,
                MatchCount = equipment.MatchCount
            });
        }
        
        // 按优先级排序
        return priorities.OrderByDescending(p => p.PriorityScore);
    }

    // 计算装备与阵容的匹配程度
    private double CalculateEquipmentMatchupScore(EquipmentData equipment, LineupData lineup)
    {
        // 这里可以根据装备的类型和阵容的特性进行更复杂的匹配度计算
        // 简化实现：基于装备在该阵容中的使用频率
        var usageCount = lineup.ChampionItems.Sum(ci => ci.Items.Count(i => i == equipment.Id));
        return Math.Min(1.0, usageCount / 3.0); // 假设一个装备最多在阵容中使用3次
    }

    // 计算阵容克制关系
    private CounterAnalysis CalculateCounterAnalysis(LineupData targetLineup, IEnumerable<LineupData> allLineups)
    {
        var counters = new List<LineupCounter>();
        var counteredBy = new List<LineupCounter>();
        
        // 简化实现：基于阵容的胜率和特性分析
        // 实际应用中，应该基于历史对战数据计算克制关系
        foreach (var otherLineup in allLineups.Where(l => l.Id != targetLineup.Id))
        {
            // 计算克制系数（这里是简化的实现）
            var counterCoefficient = CalculateCounterCoefficient(targetLineup, otherLineup);
            
            if (counterCoefficient > 1.1) // 克制
            {
                counters.Add(new LineupCounter
                {
                    LineupId = otherLineup.Id,
                    LineupName = otherLineup.Name,
                    CounterScore = counterCoefficient,
                    WinRate = otherLineup.WinRate,
                    PickRate = otherLineup.PickRate
                });
            }
            else if (counterCoefficient < 0.9) // 被克制
            {
                counteredBy.Add(new LineupCounter
                {
                    LineupId = otherLineup.Id,
                    LineupName = otherLineup.Name,
                    CounterScore = 1.0 / counterCoefficient, // 反转系数，使其表示被克制的程度
                    WinRate = otherLineup.WinRate,
                    PickRate = otherLineup.PickRate
                });
            }
        }
        
        // 按克制程度排序
        return new CounterAnalysis
        {
            LineupId = targetLineup.Id,
            Counters = counters.OrderByDescending(c => c.CounterScore).Take(5), // 取前5个克制阵容
            CounteredBy = counteredBy.OrderByDescending(c => c.CounterScore).Take(5) // 取前5个被克制阵容
        };
    }

    // 计算阵容之间的克制系数
    private double CalculateCounterCoefficient(LineupData lineupA, LineupData lineupB)
    {
        // 简化实现：基于阵容的胜率差异和特性相克关系
        // 实际应用中，应该基于历史对战数据计算
        var winRateDiff = lineupA.WinRate / lineupB.WinRate;
        
        // 特性相克分析（简化）
        var traitCounterScore = CalculateTraitCounterScore(lineupA.Traits, lineupB.Traits);
        
        // 综合克制系数
        return winRateDiff * traitCounterScore;
    }

    // 计算特性相克评分
    private double CalculateTraitCounterScore(IReadOnlyList<string> traitsA, IReadOnlyList<string> traitsB)
    {
        // 简化实现：基于特性数量和多样性
        // 实际应用中，应该基于特性之间的相克关系
        var traitDiversityA = traitsA.Count;
        var traitDiversityB = traitsB.Count;
        
        return (traitDiversityA + 1.0) / (traitDiversityB + 1.0);
    }

    public Task ClearCacheAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Clearing lineup analysis cache");
        
        // 实际实现中，应该清除所有相关的缓存键
        // 这里简化处理，让缓存自然过期
        
        return Task.CompletedTask;
    }
}
