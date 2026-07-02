using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Models.BigData;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;
using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Services;

/// <summary>
/// 装备计算服务
/// 负责实时计算装备价值、优先级和推荐
/// </summary>
public sealed class EquipmentCalculationService
{
    private readonly IBigDataService _bigDataService;
    private readonly IStaticDataProvider _staticDataProvider;
    private readonly ILogger<EquipmentCalculationService> _logger;

    public EquipmentCalculationService(
        IBigDataService bigDataService,
        IStaticDataProvider staticDataProvider,
        ILogger<EquipmentCalculationService> logger)
    {
        _bigDataService = bigDataService;
        _staticDataProvider = staticDataProvider;
        _logger = logger;
    }

    /// <summary>
    /// 计算装备的综合价值
    /// </summary>
    /// <param name="equipment">装备数据</param>
    /// <param name="context">计算上下文</param>
    /// <returns>装备价值</returns>
    public double CalculateEquipmentValue(EquipmentData equipment, EquipmentCalculationContext context)
    {
        // 基础价值：基于胜率、前四率、登顶率
        double baseValue = equipment.WinRate * 0.35 + equipment.Top4Rate * 0.3 + equipment.Top1Rate * 0.25;

        // 经济价值调整
        double economicValue = equipment.EconomicValue * 0.2;

        // 阵容适配性调整
        double lineupMatchValue = CalculateLineupMatchValue(equipment, context);

        // 装备类型调整
        double typeMultiplier = equipment.EquipmentType switch
        {
            "Artifact" => 1.5,
            "Radiant" => 1.3,
            "Completed" => 1.0,
            "Component" => 0.6,
            _ => 1.0
        };

        // 合成路径调整
        double synthesisPathValue = CalculateSynthesisPathValue(equipment, context);

        // 综合价值
        double totalValue = (baseValue + economicValue + lineupMatchValue + synthesisPathValue) * typeMultiplier;

        // 确保价值在合理范围内
        return Math.Clamp(totalValue, 0, 100);
    }

    /// <summary>
    /// 计算装备的合成路径价值
    /// </summary>
    /// <param name="equipment">装备数据</param>
    /// <param name="context">计算上下文</param>
    /// <returns>合成路径价值</returns>
    private double CalculateSynthesisPathValue(EquipmentData equipment, EquipmentCalculationContext context)
    {
        if (equipment.IsComponent)
            return 0;

        // 检查所需组件是否可用
        int availableComponents = equipment.Components.Count(c => context.AvailableComponents.Any(ac => ac.Id == c));
        double componentAvailability = availableComponents / (double)equipment.Components.Count;

        // 计算合成成本
        double synthesisCost = equipment.Components.Count * 1.0; // 每个组件成本为1

        // 合成路径价值 = 组件可用性 * (1 / 合成成本)
        return componentAvailability * (10 / synthesisCost);
    }

    /// <summary>
    /// 计算装备与阵容的匹配价值
    /// </summary>
    /// <param name="equipment">装备数据</param>
    /// <param name="context">计算上下文</param>
    /// <returns>阵容匹配价值</returns>
    private double CalculateLineupMatchValue(EquipmentData equipment, EquipmentCalculationContext context)
    {
        if (string.IsNullOrEmpty(context.LineupId))
            return 0;

        // 模拟阵容匹配度计算
        // 实际实现中，应该基于历史数据或规则计算
        var random = new Random();
        return 5 + random.NextDouble() * 10;
    }

    /// <summary>
    /// 计算装备的经济成本
    /// </summary>
    /// <param name="equipment">装备数据</param>
    /// <returns>经济成本</returns>
    public double CalculateEconomicCost(EquipmentData equipment)
    {
        if (equipment.IsComponent)
            return 1.0; // 基础组件成本为1

        // 成装成本 = 组件数量 * 1.0
        return equipment.Components.Count * 1.0;
    }

    /// <summary>
    /// 计算装备的优先级
    /// </summary>
    /// <param name="equipment">装备数据</param>
    /// <param name="context">计算上下文</param>
    /// <returns>优先级 (1-5，1最高)</returns>
    public int CalculateEquipmentPriority(EquipmentData equipment, EquipmentCalculationContext context)
    {
        double value = CalculateEquipmentValue(equipment, context);

        if (value >= 80)
            return 1;
        else if (value >= 60)
            return 2;
        else if (value >= 40)
            return 3;
        else if (value >= 20)
            return 4;
        else
            return 5;
    }

    /// <summary>
    /// 生成实时装备推荐
    /// </summary>
    /// <param name="context">计算上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备推荐列表</returns>
    public async Task<List<ItemSuggestion>> GenerateEquipmentRecommendationsAsync(EquipmentCalculationContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始生成装备推荐");

            // 获取装备数据
            var equipmentData = await _bigDataService.GetEquipmentDataAsync(context.SetVersion, false, cancellationToken);
            var staticItems = await _staticDataProvider.GetItemsAsync(context.SetVersion, cancellationToken);
            var itemMap = staticItems.ToDictionary(i => i.Id, i => i);

            // 计算每个装备的价值和优先级
            var recommendations = new List<ItemSuggestion>();

            foreach (var equipment in equipmentData)
            {
                if (itemMap.TryGetValue(equipment.Id, out var item))
                {
                    // 检查组件是否可用
                    var availableComponents = equipment.Components
                        .Where(c => context.AvailableComponents.Any(ac => ac.Id == c))
                        .ToList();

                    if (availableComponents.Count == equipment.Components.Count)
                    {
                        var priority = CalculateEquipmentPriority(equipment, context);
                        var value = CalculateEquipmentValue(equipment, context);

                        recommendations.Add(new ItemSuggestion
                        {
                            TargetChampion = context.TargetChampion,
                            Item = item,
                            Priority = priority,
                            AvailableComponents = availableComponents,
                            WinRate = equipment.WinRate,
                            Top4Rate = equipment.Top4Rate,
                            Top1Rate = equipment.Top1Rate,
                            PickRate = equipment.PickRate,
                            EconomicValue = equipment.EconomicValue,
                            MatchCount = equipment.MatchCount
                        });
                    }
                }
            }

            // 按优先级和价值排序
            recommendations.Sort((a, b) =>
            {
                int priorityComparison = a.Priority.CompareTo(b.Priority);
                if (priorityComparison != 0)
                    return priorityComparison;

                // 优先级相同时，按价值排序
                double valueA = CalculateEquipmentValue(equipmentData.First(e => e.Id == a.Item.Id), context);
                double valueB = CalculateEquipmentValue(equipmentData.First(e => e.Id == b.Item.Id), context);
                return valueB.CompareTo(valueA);
            });

            _logger.LogInformation("生成了 {Count} 个装备推荐", recommendations.Count);
            return recommendations.Take(5).ToList(); // 返回前5个推荐
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成装备推荐失败");
            return new List<ItemSuggestion>();
        }
    }

    /// <summary>
    /// 分析装备组合效果
    /// </summary>
    /// <param name="items">装备列表</param>
    /// <param name="championName">目标英雄</param>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备组合分析结果</returns>
    public async Task<ItemCombinationAnalysis> AnalyzeItemCombinationAsync(List<string> items, string championName, string setVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("分析装备组合: {Items} 用于 {Champion}", string.Join(", ", items), championName);

            // 获取装备数据
            var equipmentData = await _bigDataService.GetEquipmentDataAsync(setVersion, cancellationToken);
            var equipmentMap = equipmentData.ToDictionary(e => e.Id, e => e);

            // 计算组合的平均胜率、前四率、登顶率
            double averageWinRate = 0;
            double averageTop4Rate = 0;
            double averageTop1Rate = 0;
            double averagePickRate = 0;
            int totalMatchCount = 0;

            foreach (var itemId in items)
            {
                if (equipmentMap.TryGetValue(itemId, out var equipment))
                {
                    averageWinRate += equipment.WinRate;
                    averageTop4Rate += equipment.Top4Rate;
                    averageTop1Rate += equipment.Top1Rate;
                    averagePickRate += equipment.PickRate;
                    totalMatchCount += equipment.MatchCount;
                }
            }

            int itemCount = items.Count;
            if (itemCount > 0)
            {
                averageWinRate /= itemCount;
                averageTop4Rate /= itemCount;
                averageTop1Rate /= itemCount;
                averagePickRate /= itemCount;
            }

            // 计算组合评分
            double score = (averageWinRate * 0.4 + averageTop4Rate * 0.3 + averageTop1Rate * 0.2 + averagePickRate * 0.1) * 100;

            // 分析优势和劣势
            var strengths = AnalyzeCombinationStrengths(items);
            var weaknesses = AnalyzeCombinationWeaknesses(items);

            return new ItemCombinationAnalysis
            {
                Items = items,
                ChampionName = championName,
                WinRate = averageWinRate,
                PickRate = averagePickRate,
                MatchCount = totalMatchCount,
                Score = score,
                Strengths = strengths,
                Weaknesses = weaknesses
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析装备组合失败");
            return new ItemCombinationAnalysis
            {
                Items = items,
                ChampionName = championName,
                WinRate = 0.5,
                PickRate = 0.1,
                MatchCount = 0,
                Score = 50,
                Strengths = new List<string>(),
                Weaknesses = new List<string>()
            };
        }
    }

    /// <summary>
    /// 分析装备组合的优势
    /// </summary>
    /// <param name="items">装备列表</param>
    /// <returns>优势列表</returns>
    private List<string> AnalyzeCombinationStrengths(List<string> items)
    {
        var strengths = new List<string>();

        if (items.Contains("InfinityEdge"))
            strengths.Add("提供高额暴击伤害");
        if (items.Contains("GuinsoosRageblade"))
            strengths.Add("提供攻击速度和技能急速");
        if (items.Contains("Bloodthirster"))
            strengths.Add("提供生命偷取，增加生存能力");
        if (items.Contains("Shojin"))
            strengths.Add("提供技能急速，加快技能释放");
        if (items.Contains("BlueBuff"))
            strengths.Add("提供初始法力值，加快技能释放");
        if (items.Contains("GuardianAngel"))
            strengths.Add("提供复活效果，增加生存能力");
        if (items.Contains("Warmogs"))
            strengths.Add("提供大量生命值，增加生存能力");
        if (items.Contains("Morellonomicon"))
            strengths.Add("提供持续伤害和减治疗效果");

        return strengths;
    }

    /// <summary>
    /// 分析装备组合的劣势
    /// </summary>
    /// <param name="items">装备列表</param>
    /// <returns>劣势列表</returns>
    private List<string> AnalyzeCombinationWeaknesses(List<string> items)
    {
        var weaknesses = new List<string>();

        if (!items.Any(i => i.Contains("GuardianAngel") || i.Contains("Warmogs") || i.Contains("BrambleVest") || i.Contains("DragonClaw")))
            weaknesses.Add("缺乏生存装备");
        if (!items.Any(i => i.Contains("InfinityEdge") || i.Contains("GiantSlayer") || i.Contains("LastWhisper")))
            weaknesses.Add("缺乏输出装备");
        if (!items.Any(i => i.Contains("Shojin") || i.Contains("BlueBuff") || i.Contains("SpearOfShojin")))
            weaknesses.Add("缺乏技能急速装备");
        if (!items.Any(i => i.Contains("QuickSilver")))
            weaknesses.Add("缺乏控制免疫装备");

        return weaknesses;
    }
}

/// <summary>
/// 装备计算上下文
/// </summary>
public sealed class EquipmentCalculationContext
{
    /// <summary>
    /// 版本
    /// </summary>
    public required string SetVersion { get; init; }

    /// <summary>
    /// 目标英雄
    /// </summary>
    public required string TargetChampion { get; init; }

    /// <summary>
    /// 阵容ID
    /// </summary>
    public string? LineupId { get; init; }

    /// <summary>
    /// 可用的装备组件
    /// </summary>
    public required List<ItemComponent> AvailableComponents { get; init; }

    /// <summary>
    /// 玩家经济状况
    /// </summary>
    public int Gold { get; init; }

    /// <summary>
    /// 游戏阶段
    /// </summary>
    public string GameStage { get; init; } = "";
}
