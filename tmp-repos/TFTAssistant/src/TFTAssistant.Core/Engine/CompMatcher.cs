using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;
using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Engine;

/// <summary>
/// 阵容匹配引擎
/// 根据当前棋盘/备战席状态，从 Meta 阵容库中匹配最合适的阵容
/// </summary>
public sealed class CompMatcher : ICompMatcher
{
    private readonly ILogger<CompMatcher> _logger;
    private readonly IMetaAnalysisService _metaAnalysisService;
    private readonly IStaticDataProvider _staticDataProvider;
    private readonly IMemoryCache _cache;

    public CompMatcher(
        ILogger<CompMatcher> logger,
        IMetaAnalysisService metaAnalysisService,
        IStaticDataProvider staticDataProvider,
        IMemoryCache cache)
    {
        _logger = logger;
        _metaAnalysisService = metaAnalysisService;
        _staticDataProvider = staticDataProvider;
        _cache = cache;
    }

    public async Task<List<CompSuggestion>> MatchAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<BenchUnit> bench,
        IReadOnlyList<Augment> augments,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey(board, bench, augments);
        
        // 尝试从缓存获取
        if (_cache.TryGetValue(cacheKey, out List<CompSuggestion> cachedResult))
        {
            _logger.LogInformation("阵容匹配结果从缓存获取");
            return cachedResult;
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // 提取当前特征
            var ownedChampions = board.Concat(bench)
                .Select(u => u.ChampionName)
                .Distinct()
                .ToHashSet();

            // 并行计算活跃羁绊和棋子星级
            var activeTraitsTask = CalculateActiveTraits(board, cancellationToken);
            var championStars = GetChampionStars(board, bench); // 同步计算，因为数据量小

            // 从真实 Meta 数据获取阵容信息（带缓存）
            var allCompsTask = GetMetaCompsWithCache(cancellationToken);

            // 等待所有异步任务完成
            await Task.WhenAll(activeTraitsTask, allCompsTask);
            
            var activeTraits = await activeTraitsTask;
            var allComps = await allCompsTask;

            if (allComps == null || allComps.Count == 0)
            {
                _logger.LogWarning("No meta comps available, using fallback");
                allComps = GenerateFallbackComps();
            }

            // 评分排序 - 使用并行处理提高性能
            var scored = allComps
                .AsParallel()
                .Select(comp => ScoreComp(comp, ownedChampions, activeTraits, augments, championStars))
                .OrderByDescending(s => s.Score)
                .Take(3)
                .ToList();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            _logger.LogInformation($"阵容匹配耗时: {elapsedMs}ms");

            // 缓存结果，有效期根据生成时间动态调整
            var cacheDuration = Math.Max(5, Math.Min(20, 20 - elapsedMs / 10)); // 5-20秒
            _cache.Set(cacheKey, scored, TimeSpan.FromSeconds(cacheDuration));
            _logger.LogDebug("阵容匹配结果已缓存，有效期: {CacheDuration}秒", cacheDuration);

            return scored;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to match comps");
            // 返回 fallback 结果
            var fallbackComps = GenerateFallbackComps();
            var ownedChampions = board.Concat(bench)
                .Select(u => u.ChampionName)
                .Distinct()
                .ToHashSet();
            var activeTraits = new Dictionary<string, int>();
            var championStars = GetChampionStars(board, bench);

            var fallbackResult = fallbackComps
                .AsParallel()
                .Select(comp => ScoreComp(comp, ownedChampions, activeTraits, augments, championStars))
                .OrderByDescending(s => s.Score)
                .Take(3)
                .ToList();

            // 缓存 fallback 结果
            _cache.Set(cacheKey, fallbackResult, TimeSpan.FromSeconds(10));

            return fallbackResult;
        }
    }

    private string GenerateCacheKey(IReadOnlyList<BoardUnit> board, IReadOnlyList<BenchUnit> bench, IReadOnlyList<Augment> augments)
    {
        var keyBuilder = new System.Text.StringBuilder();
        
        // 添加棋盘单位信息
        foreach (var unit in board.OrderBy(u => u.ChampionName))
        {
            keyBuilder.Append($"board:{unit.ChampionName}:{unit.StarLevel}_");
        }
        
        // 添加备战席单位信息
        foreach (var unit in bench.OrderBy(u => u.ChampionName))
        {
            keyBuilder.Append($"bench:{unit.ChampionName}:{unit.StarLevel}_");
        }
        
        // 添加已选强化符文
        foreach (var augment in augments.OrderBy(a => a.Id))
        {
            keyBuilder.Append($"augment:{augment.Id}_");
        }
        
        return keyBuilder.ToString();
    }

    private async Task<List<MetaComp>> GetMetaCompsWithCache(CancellationToken cancellationToken)
    {
        const string cacheKey = "meta_comps";
        
        // 尝试从缓存获取
        if (_cache.TryGetValue(cacheKey, out List<MetaComp> cachedComps))
        {
            return cachedComps;
        }
        
        // 从服务获取
        var comps = await _metaAnalysisService.GetMetaCompsAsync(cancellationToken);
        
        // 缓存结果，有效期30秒
        if (comps != null && comps.Count > 0)
        {
            _cache.Set(cacheKey, comps, TimeSpan.FromSeconds(30));
        }
        
        return comps;
    }

    private CompSuggestion ScoreComp(
        MetaComp comp,
        HashSet<string> owned,
        Dictionary<string, int> traits,
        IReadOnlyList<Augment> augments,
        Dictionary<string, int> championStars)
    {
        // 已拥有棋子匹配度 (权重 30%)
        double ownedScore = comp.Units.Count(u => owned.Contains(u))
                            / (double)comp.Units.Count * 30;

        // 棋子星级加成 (权重 10%)
        double starScore = comp.Units
            .Where(u => owned.Contains(u))
            .Sum(u => championStars.GetValueOrDefault(u, 1) - 1) / (double)(comp.Units.Count * 2) * 10;

        // 羁绊激活度 (权重 25%)
        double traitScore = 0;
        foreach (var trait in comp.Traits)
        {
            int activeCount = traits.GetValueOrDefault(trait, 0);
            // 基于实际激活数量计算分数
            traitScore += Math.Min(activeCount / (double)3, 1); // 假设3个为满激活
        }
        traitScore = traitScore / Math.Max(1, comp.Traits.Count) * 25;

        // 强化符文契合度 (权重 15%)
        double augmentScore = comp.SynergyAugments.Count(a =>
            augments.Any(ownedAugment => ownedAugment.Id.Contains(a, StringComparison.OrdinalIgnoreCase)))
            / (double)Math.Max(1, comp.SynergyAugments.Count) * 15;

        // 版本强度 (权重 20%)
        double tierScore = comp.TierScore * 20;

        var totalScore = ownedScore + starScore + traitScore + augmentScore + tierScore;

        return new CompSuggestion
        {
            Comp = comp,
            Score = Math.Round(totalScore, 1),
            OwnedUnits = comp.Units
                .Where(u => owned.Contains(u))
                .ToList(),
            MissingUnits = comp.Units
                .Where(u => !owned.Contains(u))
                .ToList(),
            TraitCoverage = Math.Round(traitScore / 25, 2),
            Reasoning = GenerateReasoning(comp, owned, traits, augments, championStars, totalScore)
        };
    }

    private async Task<Dictionary<string, int>> CalculateActiveTraits(
        IReadOnlyList<BoardUnit> board,
        CancellationToken cancellationToken)
    {
        var traits = new Dictionary<string, int>();
        try
        {
            // 从静态数据获取英雄-羁绊映射
            var champions = await _staticDataProvider.GetChampionsAsync("latest", cancellationToken);
            var championTraitMap = champions.ToDictionary(c => c.Name, c => c.Traits);

            foreach (var unit in board)
            {
                if (championTraitMap.TryGetValue(unit.ChampionName, out var unitTraits))
                {
                    foreach (var trait in unitTraits)
                    {
                        traits[trait] = traits.GetValueOrDefault(trait, 0) + 1;
                    }
                }
                else
                {
                    // 回退：使用默认羁绊
                    AddDefaultTraits(unit.ChampionName, traits);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate active traits, using default");
            // 回退：使用默认羁绊
            foreach (var unit in board)
            {
                AddDefaultTraits(unit.ChampionName, traits);
            }
        }
        return traits;
    }

    private void AddDefaultTraits(string championName, Dictionary<string, int> traits)
    {
        // 简化处理，实际需从静态数据获取
        if (championName == "Ahri")
        {
            traits["Mage"] = traits.GetValueOrDefault("Mage", 0) + 1;
            traits["Syndicate"] = traits.GetValueOrDefault("Syndicate", 0) + 1;
        }
        else if (championName == "Akali")
        {
            traits["Assassin"] = traits.GetValueOrDefault("Assassin", 0) + 1;
            traits["Syndicate"] = traits.GetValueOrDefault("Syndicate", 0) + 1;
        }
        else if (championName == "Ashe")
        {
            traits["Marksman"] = traits.GetValueOrDefault("Marksman", 0) + 1;
            traits["Freljord"] = traits.GetValueOrDefault("Freljord", 0) + 1;
        }
        else if (championName == "Draven")
        {
            traits["Gladiator"] = traits.GetValueOrDefault("Gladiator", 0) + 1;
            traits["Noxus"] = traits.GetValueOrDefault("Noxus", 0) + 1;
        }
        else if (championName == "Kai'Sa")
        {
            traits["Assassin"] = traits.GetValueOrDefault("Assassin", 0) + 1;
            traits["Void"] = traits.GetValueOrDefault("Void", 0) + 1;
        }
        else if (championName == "Leona")
        {
            traits["Guardian"] = traits.GetValueOrDefault("Guardian", 0) + 1;
            traits["Dawnbringer"] = traits.GetValueOrDefault("Dawnbringer", 0) + 1;
        }
        else if (championName == "Lux")
        {
            traits["Mage"] = traits.GetValueOrDefault("Mage", 0) + 1;
            traits["Dawnbringer"] = traits.GetValueOrDefault("Dawnbringer", 0) + 1;
        }
        else if (championName == "MissFortune")
        {
            traits["Marksman"] = traits.GetValueOrDefault("Marksman", 0) + 1;
            traits["Bilgewater"] = traits.GetValueOrDefault("Bilgewater", 0) + 1;
        }
        else if (championName == "Sett")
        {
            traits["Brawler"] = traits.GetValueOrDefault("Brawler", 0) + 1;
            traits["Void"] = traits.GetValueOrDefault("Void", 0) + 1;
        }
        else if (championName == "Syndra")
        {
            traits["Mage"] = traits.GetValueOrDefault("Mage", 0) + 1;
            traits["Coven"] = traits.GetValueOrDefault("Coven", 0) + 1;
        }
    }

    private Dictionary<string, int> GetChampionStars(IReadOnlyList<BoardUnit> board, IReadOnlyList<BenchUnit> bench)
    {
        var stars = new Dictionary<string, int>();
        
        foreach (var unit in board.Concat(bench))
        {
            stars[unit.ChampionName] = Math.Max(stars.GetValueOrDefault(unit.ChampionName, 0), unit.StarLevel);
        }
        
        return stars;
    }

    private List<MetaComp> GenerateFallbackComps()
    {
        return new List<MetaComp>
        {
            new()
            {
                Id = "mage_syndicate",
                Name = "法师 Syndicate",
                Units = new List<string> { "Ahri", "Syndra", "Lux", "Zoe", "Lissandra", "Galio" },
                Traits = new List<string> { "Mage", "Syndicate", "Covenant" },
                SynergyAugments = new List<string> { "Mage's Might", "Syndicate Emblem" },
                TierScore = 0.8
            },
            new()
            {
                Id = "assassin_syndicate",
                Name = "刺客 Syndicate",
                Units = new List<string> { "Akali", "Katarina", "Evelynn", "Pyke", "Twisted Fate", "Jhin" },
                Traits = new List<string> { "Assassin", "Syndicate", "Nightfall" },
                SynergyAugments = new List<string> { "Assassin's Greed", "Syndicate Emblem" },
                TierScore = 0.75
            },
            new()
            {
                Id = "marksman_freljord",
                Name = "射手弗雷尔卓德",
                Units = new List<string> { "Ashe", "Caitlyn", "Jhin", "Varus", "Sejuani", "Braum" },
                Traits = new List<string> { "Marksman", "Freljord", "Guardian" },
                SynergyAugments = new List<string> { "Marksman's Focus", "Freljord Emblem" },
                TierScore = 0.7
            },
            new()
            {
                Id = "gladiator_noxus",
                Name = "角斗士诺克萨斯",
                Units = new List<string> { "Draven", "Darius", "Kled", "Sion", "Swain", "Garen" },
                Traits = new List<string> { "Gladiator", "Noxus", "Legionnaire" },
                SynergyAugments = new List<string> { "Gladiator's Might", "Noxus Emblem" },
                TierScore = 0.72
            },
            new()
            {
                Id = "void_assassin",
                Name = "虚空刺客",
                Units = new List<string> { "Kai'Sa", "Evelynn", "Kha'Zix", "Rengar", "Cho'Gath", "Vel'Koz" },
                Traits = new List<string> { "Void", "Assassin", "Reckoner" },
                SynergyAugments = new List<string> { "Void Dominance", "Assassin Emblem" },
                TierScore = 0.78
            }
        };
    }

    private string GenerateReasoning(
        MetaComp comp,
        HashSet<string> owned,
        Dictionary<string, int> traits,
        IReadOnlyList<Augment> augments,
        Dictionary<string, int> championStars,
        double totalScore)
    {
        var reasons = new List<string>();
        
        // 棋子匹配度
        int ownedCount = comp.Units.Count(u => owned.Contains(u));
        if (ownedCount >= comp.Units.Count * 0.6)
        {
            reasons.Add($"已拥有 {ownedCount}/{comp.Units.Count} 个核心棋子");
        }
        
        // 高星棋子
        var highStarChampions = comp.Units
            .Where(u => owned.Contains(u) && championStars.GetValueOrDefault(u, 1) >= 2)
            .ToList();
        if (highStarChampions.Count > 0)
        {
            reasons.Add($"包含 {highStarChampions.Count} 个高星核心棋子");
        }
        
        // 羁绊激活
        int activeTraitsCount = comp.Traits.Count(t => traits.GetValueOrDefault(t, 0) >= 1);
        if (activeTraitsCount >= comp.Traits.Count * 0.5)
        {
            reasons.Add($"已激活 {activeTraitsCount}/{comp.Traits.Count} 个核心羁绊");
        }
        
        // 强化符文契合
        int synergyAugmentsCount = comp.SynergyAugments.Count(a =>
            augments.Any(ownedAugment => ownedAugment.Id.Contains(a, StringComparison.OrdinalIgnoreCase)));
        if (synergyAugmentsCount > 0)
        {
            reasons.Add($"拥有 {synergyAugmentsCount} 个契合的强化符文");
        }
        
        // 版本强度
        if (comp.TierScore >= 0.8)
        {
            reasons.Add("当前版本强势阵容");
        }
        else if (comp.TierScore >= 0.7)
        {
            reasons.Add("当前版本较为强势的阵容");
        }
        
        if (reasons.Count == 0)
        {
            reasons.Add("基于当前游戏状态的推荐阵容");
        }
        
        return string.Join("；", reasons);
    }
}
