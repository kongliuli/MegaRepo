using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;
using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Engine;

/// <summary>
/// 装备建议引擎
/// 基于大数据计算装备优先级，分析装备组合效果，为不同阵容提供针对性的装备选择建议
/// </summary>
public sealed class ItemAdvisor : IItemAdvisor
{
    private readonly IBigDataService _bigDataService;
    private readonly IStaticDataProvider _staticData;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ItemAdvisor> _logger;

    public ItemAdvisor(
        IBigDataService bigDataService,
        IStaticDataProvider staticData,
        IMemoryCache cache,
        ILogger<ItemAdvisor> logger)
    {
        _bigDataService = bigDataService;
        _staticData = staticData;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ItemSuggestion>> AdviseAsync(
            IReadOnlyList<BoardUnit> board,
            IReadOnlyList<ItemComponent> availableComponents,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = GenerateCacheKey(board, availableComponents);
            
            // 尝试从缓存获取
            if (_cache.TryGetValue(cacheKey, out List<ItemSuggestion> cachedResult))
            {
                _logger.LogInformation("装备推荐结果从缓存获取");
                return cachedResult;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            var suggestions = new List<ItemSuggestion>();
            var usedComponents = new HashSet<string>();
            var setVersion = "set13"; // 实际应从游戏状态获取

            // 按优先级处理：主 C（3星） > 副 C（2星） > 其他
            var sortedUnits = board
                .Where(u => u.StarLevel >= 1) // 包括1星英雄，确保更多选择
                .OrderByDescending(u => u.StarLevel)
                .ThenByDescending(u => u.Items.Count)
                .ToList();

            // 获取所有装备数据（带缓存）
            var equipmentMap = await GetEquipmentMapWithCache(setVersion, cancellationToken);

            // 统计可用组件
            var componentCount = availableComponents.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.Count());

            foreach (var unit in sortedUnits)
            {
                // 获取英雄最佳装备（带缓存）
                var bestItems = await GetBestItemsForWithCache(unit.ChampionName, setVersion, cancellationToken);

                foreach (var item in bestItems)
                {
                    // 检查组件是否可用且未被分配
                    var neededComponents = new List<string>();
                    var tempComponentCount = new Dictionary<string, int>(componentCount);
                    
                    bool componentsAvailable = true;
                    foreach (var component in item.Components)
                    {
                        if (tempComponentCount.TryGetValue(component, out int count) && count > 0)
                        {
                            neededComponents.Add(component);
                            tempComponentCount[component]--;
                        }
                        else
                        {
                            componentsAvailable = false;
                            break;
                        }
                    }

                    if (componentsAvailable)
                    {
                        var equipmentData = equipmentMap.GetValueOrDefault(item.Id);
                        var priority = CalculatePriority(equipmentData);
                        
                        // 计算装备组合协同效应
                        var synergyScore = CalculateItemSynergy(item.Id, unit.ChampionName, unit.Items);
                        
                        suggestions.Add(new ItemSuggestion
                        {
                            TargetChampion = unit.ChampionName,
                            Item = item,
                            Priority = priority,
                            SynergyScore = synergyScore,
                            AvailableComponents = neededComponents,
                            WinRate = equipmentData?.WinRate ?? 0,
                            Top4Rate = equipmentData?.Top4Rate ?? 0,
                            Top1Rate = equipmentData?.Top1Rate ?? 0,
                            PickRate = equipmentData?.PickRate ?? 0,
                            EconomicValue = equipmentData?.EconomicValue ?? 0,
                            MatchCount = equipmentData?.MatchCount ?? 0,
                            Reasoning = GenerateItemReasoning(item, equipmentData, synergyScore, unit)
                        });

                        // 标记组件为已使用
                        foreach (var c in neededComponents)
                        {
                            componentCount[c]--;
                            if (componentCount[c] == 0)
                                componentCount.Remove(c);
                        }
                    }
                }
            }

            var result = suggestions
                .OrderByDescending(s => s.Priority)
                .ThenByDescending(s => s.SynergyScore)
                .ThenByDescending(s => s.WinRate)
                .Take(5)
                .ToList();

            stopwatch.Stop();
            _logger.LogInformation($"装备推荐耗时: {stopwatch.ElapsedMilliseconds}ms");

            // 缓存结果，有效期10秒
            _cache.Set(cacheKey, result, TimeSpan.FromSeconds(10));

            return result;
        }

        private string GenerateCacheKey(IReadOnlyList<BoardUnit> board, IReadOnlyList<ItemComponent> availableComponents)
        {
            var keyBuilder = new System.Text.StringBuilder();
            
            // 添加棋盘单位信息
            foreach (var unit in board.OrderBy(u => u.ChampionName))
            {
                keyBuilder.Append($"unit:{unit.ChampionName}:{unit.StarLevel}:{string.Join(",", unit.Items.OrderBy(i => i))}_");
            }
            
            // 添加可用组件信息
            foreach (var component in availableComponents.OrderBy(c => c.Id))
            {
                keyBuilder.Append($"component:{component.Id}_");
            }
            
            return keyBuilder.ToString();
        }

        private async Task<Dictionary<string, EquipmentData>> GetEquipmentMapWithCache(string setVersion, CancellationToken cancellationToken)
        {
            var cacheKey = $"equipment_map:{setVersion}";
            
            // 尝试从缓存获取
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, EquipmentData> cachedMap))
            {
                return cachedMap;
            }
            
            // 从服务获取
            var allEquipment = await _bigDataService.GetEquipmentDataAsync(setVersion, false, cancellationToken);
            var equipmentMap = allEquipment.ToDictionary(e => e.Id, e => e);
            
            // 缓存结果，有效期60秒
            _cache.Set(cacheKey, equipmentMap, TimeSpan.FromSeconds(60));
            
            return equipmentMap;
        }

        private async Task<List<Item>> GetBestItemsForWithCache(string championName, string setVersion, CancellationToken cancellationToken)
        {
            var cacheKey = $"best_items:{championName}:{setVersion}";
            
            // 尝试从缓存获取
            if (_cache.TryGetValue(cacheKey, out List<Item> cachedItems))
            {
                return cachedItems;
            }
            
            // 从装备推荐规则表查询
            var builds = ItemBuildDatabase.Get(championName);
            if (builds.Count > 0)
            {
                // 缓存结果，有效期300秒
                _cache.Set(cacheKey, builds, TimeSpan.FromSeconds(300));
                return builds;
            }

            // 回退：从静态数据获取所有成品装备
            var allItems = await _staticData.GetItemsAsync(setVersion, cancellationToken);
            var finishedItems = allItems.Where(i => !i.IsComponent).ToList();
            
            // 缓存结果，有效期300秒
            _cache.Set(cacheKey, finishedItems, TimeSpan.FromSeconds(300));
            
            return finishedItems;
        }

    public async Task<ItemCombinationAnalysis> AnalyzeItemCombinationAsync(
        List<string> items,
        string championName,
        CancellationToken cancellationToken = default)
    {
        var setVersion = "set13"; // 实际应从游戏状态获取
        var equipmentDataList = await _bigDataService.GetEquipmentDataAsync(setVersion, false, cancellationToken);
        var equipmentMap = equipmentDataList.ToDictionary(e => e.Id, e => e);

        // 计算组合的平均胜率和选择率
        double totalWinRate = 0;
        double totalPickRate = 0;
        int totalMatchCount = 0;
        int validItems = 0;

        foreach (var itemId in items)
        {
            if (equipmentMap.TryGetValue(itemId, out var equipment))
            {
                totalWinRate += equipment.WinRate;
                totalPickRate += equipment.PickRate;
                totalMatchCount += equipment.MatchCount;
                validItems++;
            }
        }

        double avgWinRate = validItems > 0 ? totalWinRate / validItems : 0;
        double avgPickRate = validItems > 0 ? totalPickRate / validItems : 0;
        
        // 计算组合评分
        double score = CalculateCombinationScore(avgWinRate, avgPickRate, totalMatchCount);

        // 分析优势和劣势
        var strengths = AnalyzeStrengths(items, championName);
        var weaknesses = AnalyzeWeaknesses(items, championName);

        return new ItemCombinationAnalysis
        {
            Items = items,
            ChampionName = championName,
            WinRate = avgWinRate,
            PickRate = avgPickRate,
            MatchCount = totalMatchCount,
            Score = score,
            Strengths = strengths,
            Weaknesses = weaknesses
        };
    }

    public async Task<LineupItemRecommendation> GetLineupItemRecommendationAsync(
        string lineupId,
        string setVersion,
        CancellationToken cancellationToken = default)
    {
        // 获取阵容数据
        var lineups = await _bigDataService.GetLineupsAsync(setVersion, "all", cancellationToken);
        var targetLineup = lineups.FirstOrDefault(l => l.Id == lineupId);

        if (targetLineup == null)
        {
            _logger.LogWarning("Lineup not found: {LineupId}", lineupId);
            return new LineupItemRecommendation
            {
                LineupId = lineupId,
                LineupName = "Unknown Lineup",
                SetVersion = setVersion,
                ChampionRecommendations = new List<ChampionItemRecommendation>(),
                CoreItemPriorities = new List<ItemPriority>()
            };
        }

        // 获取装备数据
        var equipmentData = await _bigDataService.GetEquipmentDataAsync(setVersion, false, cancellationToken);
        var equipmentMap = equipmentData.ToDictionary(e => e.Id, e => e);

        // 为每个英雄生成装备推荐
        var championRecommendations = new List<ChampionItemRecommendation>();
        var allItemPriorities = new List<ItemPriority>();

        foreach (var champion in targetLineup.Champions)
        {
            var bestItems = await GetBestItemsFor(champion, setVersion, cancellationToken);
            var recommendedItems = new List<RecommendedItem>();
            var itemPriorities = new List<ItemPriority>();

            foreach (var item in bestItems.Take(3)) // 每个英雄最多推荐3件装备
            {
                if (equipmentMap.TryGetValue(item.Id, out var equipment))
                {
                    recommendedItems.Add(new RecommendedItem
                    {
                        ItemId = item.Id,
                        ItemName = item.Name,
                        Priority = CalculatePriority(equipment),
                        WinRate = equipment.WinRate,
                        PickRate = equipment.PickRate
                    });

                    itemPriorities.Add(new ItemPriority
                    {
                        ItemId = item.Id,
                        ItemName = item.Name,
                        PriorityScore = CalculatePriorityScore(equipment),
                        SuitableChampions = new List<string> { champion }
                    });
                }
            }

            championRecommendations.Add(new ChampionItemRecommendation
            {
                ChampionName = champion,
                RecommendedItems = recommendedItems,
                ItemPriorities = itemPriorities
            });

            allItemPriorities.AddRange(itemPriorities);
        }

        // 计算核心装备优先级
        var coreItemPriorities = allItemPriorities
            .GroupBy(ip => ip.ItemId)
            .Select(g => new ItemPriority
            {
                ItemId = g.Key,
                ItemName = g.First().ItemName,
                PriorityScore = g.Average(ip => ip.PriorityScore),
                SuitableChampions = g.SelectMany(ip => ip.SuitableChampions).Distinct().ToList()
            })
            .OrderByDescending(ip => ip.PriorityScore)
            .Take(5)
            .ToList();

        return new LineupItemRecommendation
        {
            LineupId = lineupId,
            LineupName = targetLineup.Name,
            SetVersion = setVersion,
            ChampionRecommendations = championRecommendations,
            CoreItemPriorities = coreItemPriorities
        };
    }

    private async Task<List<Item>> GetBestItemsFor(string championName, string setVersion, CancellationToken cancellationToken)
    {
        // 从装备推荐规则表查询
        var builds = ItemBuildDatabase.Get(championName);
        if (builds.Count > 0)
            return builds;

        // 回退：从静态数据获取所有成品装备
        var allItems = await _staticData.GetItemsAsync(setVersion, cancellationToken);
        return allItems.Where(i => !i.IsComponent).ToList();
    }

    private int CalculatePriority(EquipmentData? equipment)
    {
        if (equipment == null)
            return 5; // 最低优先级

        // 基于胜率、前四率、登顶率、选择率和经济价值计算优先级
        double score = (equipment.WinRate * 0.3 + 
                       equipment.Top4Rate * 0.25 + 
                       equipment.Top1Rate * 0.2 + 
                       equipment.PickRate * 0.15 + 
                       equipment.EconomicValue * 0.1) * 100;
        
        if (score >= 70)
            return 1;
        else if (score >= 50)
            return 2;
        else if (score >= 30)
            return 3;
        else if (score >= 10)
            return 4;
        else
            return 5;
    }

    private double CalculatePriorityScore(EquipmentData equipment)
    {
        // 综合考虑胜率、前四率、登顶率、选择率、经济价值和场次
        double winRateWeight = 0.3;
        double top4RateWeight = 0.25;
        double top1RateWeight = 0.2;
        double pickRateWeight = 0.15;
        double economicValueWeight = 0.1;
        double matchCountWeight = 0.1;

        // 场次归一化（最大场次为10000）
        double normalizedMatchCount = Math.Min(equipment.MatchCount / 10000.0, 1.0);

        return (equipment.WinRate * winRateWeight + 
                equipment.Top4Rate * top4RateWeight + 
                equipment.Top1Rate * top1RateWeight + 
                equipment.PickRate * pickRateWeight * 10 + // 选择率放大10倍
                equipment.EconomicValue * economicValueWeight * 10 + // 经济价值放大10倍
                normalizedMatchCount * matchCountWeight) * 100;
    }

    private double CalculateCombinationScore(double winRate, double pickRate, int matchCount)
    {
        // 基于胜率、选择率和场次计算组合评分
        double baseScore = winRate * 70 + pickRate * 30 * 10;
        
        // 场次权重（至少需要100场才有完整权重）
        double matchWeight = Math.Min(matchCount / 100.0, 1.0);
        
        return baseScore * matchWeight;
    }

    private List<string> AnalyzeStrengths(List<string> items, string championName)
    {
        var strengths = new List<string>();
        
        // 简单的装备组合优势分析
        if (items.Contains("InfinityEdge"))
            strengths.Add("提供高额暴击伤害");
        if (items.Contains("Shojin"))
            strengths.Add("提供技能急速");
        if (items.Contains("GiantSlayer"))
            strengths.Add("对高生命值目标造成额外伤害");
        if (items.Contains("GuardianAngel"))
            strengths.Add("提供复活效果，增加生存能力");
        if (items.Contains("Morellonomicon"))
            strengths.Add("提供持续伤害和减治疗效果");
        
        return strengths;
    }

    private List<string> AnalyzeWeaknesses(List<string> items, string championName)
    {
        var weaknesses = new List<string>();
        
        // 简单的装备组合劣势分析
        if (!items.Any(i => i.Contains("GuardianAngel") || i.Contains("BrambleVest") || i.Contains("DragonClaw")))
            weaknesses.Add("缺乏生存装备");
        if (!items.Any(i => i.Contains("InfinityEdge") || i.Contains("GiantSlayer") || i.Contains("LastWhisper")))
            weaknesses.Add("缺乏输出装备");
        if (!items.Any(i => i.Contains("Shojin") || i.Contains("BlueBuff") || i.Contains("SpearOfShojin")))
            weaknesses.Add("缺乏技能急速装备");
        
        return weaknesses;
    }

    private double CalculateItemSynergy(string itemId, string championName, IReadOnlyList<string> existingItems)
    {
        double synergyScore = 0;
        
        // 基于英雄类型的装备协同
        if (IsAPChampion(championName))
        {
            if (itemId.Contains("Rod") || itemId.Contains("Tear"))
                synergyScore += 20;
        }
        else if (IsADChampion(championName))
        {
            if (itemId.Contains("Sword") || itemId.Contains("Bow"))
                synergyScore += 20;
        }
        else if (IsTankChampion(championName))
        {
            if (itemId.Contains("Belt") || itemId.Contains("Chain"))
                synergyScore += 20;
        }
        
        // 装备组合协同
        if (existingItems.Contains("Shojin") && itemId == "BlueBuff")
            synergyScore += 15; // 技能急速组合
        if (existingItems.Contains("InfinityEdge") && itemId == "GuinsoosRageblade")
            synergyScore += 15; // 暴击攻速组合
        if (existingItems.Contains("WarmogsArmor") && itemId == "BrambleVest")
            synergyScore += 15; // 坦克生存组合
        
        // 装备功能协同
        if (itemId == "GuardianAngel" && existingItems.Any(i => i.Contains("Damage")))
            synergyScore += 10; // 输出+生存
        if (itemId == "Morellonomicon" && existingItems.Any(i => i.Contains("Rod")))
            synergyScore += 10; // AP+减治疗
        
        return synergyScore;
    }

    private bool IsAPChampion(string championName)
    {
        var apChampions = new HashSet<string> { "Ahri", "Syndra", "Lux", "Zoe", "Lissandra" };
        return apChampions.Contains(championName);
    }

    private bool IsADChampion(string championName)
    {
        var adChampions = new HashSet<string> { "Akali", "Draven", "Jhin", "Kai'Sa", "MissFortune" };
        return adChampions.Contains(championName);
    }

    private bool IsTankChampion(string championName)
    {
        var tankChampions = new HashSet<string> { "Leona", "Sett", "Braum", "Sejuani" };
        return tankChampions.Contains(championName);
    }

    private string GenerateItemReasoning(Item item, EquipmentData? equipmentData, double synergyScore, BoardUnit unit)
    {
        var reasons = new List<string>();
        
        // 基于装备数据的理由
        if (equipmentData != null)
        {
            if (equipmentData.WinRate >= 0.55)
                reasons.Add($"胜率较高 ({Math.Round(equipmentData.WinRate * 100, 1)}%)");
            if (equipmentData.PickRate >= 0.1)
                reasons.Add($"选择率较高 ({Math.Round(equipmentData.PickRate * 100, 1)}%)");
            if (equipmentData.MatchCount >= 1000)
                reasons.Add("统计样本充足");
        }
        
        // 基于协同效应的理由
        if (synergyScore >= 20)
            reasons.Add("与现有装备/英雄高度契合");
        else if (synergyScore >= 10)
            reasons.Add("与现有装备/英雄有良好协同");
        
        // 基于英雄类型的理由
        if (IsAPChampion(unit.ChampionName) && item.Components.Any(c => c == "Rod" || c == "Tear"))
            reasons.Add("适合AP英雄的装备");
        else if (IsADChampion(unit.ChampionName) && item.Components.Any(c => c == "Sword" || c == "Bow"))
            reasons.Add("适合AD英雄的装备");
        else if (IsTankChampion(unit.ChampionName) && item.Components.Any(c => c == "Belt" || c == "Chain"))
            reasons.Add("适合坦克英雄的装备");
        
        // 基于装备功能的理由
        if (item.Id.Contains("GuardianAngel"))
            reasons.Add("提供复活效果，增加生存能力");
        else if (item.Id.Contains("Morellonomicon"))
            reasons.Add("提供持续伤害和减治疗效果");
        else if (item.Id.Contains("InfinityEdge"))
            reasons.Add("提供高额暴击伤害");
        else if (item.Id.Contains("Shojin"))
            reasons.Add("提供技能急速，加快技能释放");
        
        if (reasons.Count == 0)
        {
            reasons.Add("基于当前游戏状态的推荐装备");
        }
        
        return string.Join("；", reasons);
    }
}
