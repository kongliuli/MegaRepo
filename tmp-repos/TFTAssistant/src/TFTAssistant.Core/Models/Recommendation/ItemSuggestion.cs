using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Models.Recommendation;

/// <summary>
/// 装备建议
/// </summary>
public sealed class ItemSuggestion
{
    /// <summary>
    /// 目标英雄
    /// </summary>
    public required string TargetChampion { get; init; }
    
    /// <summary>
    /// 装备
    /// </summary>
    public required Item Item { get; init; }
    
    /// <summary>
    /// 优先级 (1-5，1最高)
    /// </summary>
    public required int Priority { get; init; }
    
    /// <summary>
    /// 可用的装备组件
    /// </summary>
    public required List<string> AvailableComponents { get; init; }
    
    /// <summary>
    /// 装备胜率
    /// </summary>
    public double WinRate { get; init; }
    
    /// <summary>
    /// 前四率
    /// </summary>
    public double Top4Rate { get; init; }
    
    /// <summary>
    /// 登顶率
    /// </summary>
    public double Top1Rate { get; init; }
    
    /// <summary>
    /// 装备选择率
    /// </summary>
    public double PickRate { get; init; }
    
    /// <summary>
    /// 经济价值
    /// </summary>
    public double EconomicValue { get; init; }
    
    /// <summary>
    /// 统计场次
    /// </summary>
    public int MatchCount { get; init; }
    
    /// <summary>
    /// 分析来源
    /// </summary>
    public string AnalysisSource { get; init; } = "历史统计数据";
    
    /// <summary>
    /// 装备组合协同效应评分
    /// </summary>
    public double SynergyScore { get; init; }
    
    /// <summary>
    /// 推荐理由
    /// </summary>
    public string Reasoning { get; init; } = string.Empty;
}

/// <summary>
/// 装备组合分析结果
/// </summary>
public sealed class ItemCombinationAnalysis
{
    /// <summary>
    /// 装备组合
    /// </summary>
    public required List<string> Items { get; init; }
    
    /// <summary>
    /// 目标英雄
    /// </summary>
    public required string ChampionName { get; init; }
    
    /// <summary>
    /// 组合胜率
    /// </summary>
    public double WinRate { get; init; }
    
    /// <summary>
    /// 组合选择率
    /// </summary>
    public double PickRate { get; init; }
    
    /// <summary>
    /// 统计场次
    /// </summary>
    public int MatchCount { get; init; }
    
    /// <summary>
    /// 组合评分 (0-100)
    /// </summary>
    public double Score { get; init; }
    
    /// <summary>
    /// 优势分析
    /// </summary>
    public List<string> Strengths { get; init; } = new List<string>();
    
    /// <summary>
    /// 劣势分析
    /// </summary>
    public List<string> Weaknesses { get; init; } = new List<string>();
    
    /// <summary>
    /// 分析来源
    /// </summary>
    public string AnalysisSource { get; init; } = "历史统计数据";
}

/// <summary>
/// 阵容装备推荐
/// </summary>
public sealed class LineupItemRecommendation
{
    /// <summary>
    /// 阵容ID
    /// </summary>
    public required string LineupId { get; init; }
    
    /// <summary>
    /// 阵容名称
    /// </summary>
    public required string LineupName { get; init; }
    
    /// <summary>
    /// 版本
    /// </summary>
    public required string SetVersion { get; init; }
    
    /// <summary>
    /// 英雄装备推荐
    /// </summary>
    public required List<ChampionItemRecommendation> ChampionRecommendations { get; init; }
    
    /// <summary>
    /// 核心装备优先级
    /// </summary>
    public required List<ItemPriority> CoreItemPriorities { get; init; }
    
    /// <summary>
    /// 分析来源
    /// </summary>
    public string AnalysisSource { get; init; } = "历史统计数据";
}

/// <summary>
/// 英雄装备推荐
/// </summary>
public sealed class ChampionItemRecommendation
{
    /// <summary>
    /// 英雄名称
    /// </summary>
    public required string ChampionName { get; init; }
    
    /// <summary>
    /// 推荐装备
    /// </summary>
    public required List<RecommendedItem> RecommendedItems { get; init; }
    
    /// <summary>
    /// 装备优先级
    /// </summary>
    public required List<ItemPriority> ItemPriorities { get; init; }
}

/// <summary>
/// 推荐装备
/// </summary>
public sealed class RecommendedItem
{
    /// <summary>
    /// 装备ID
    /// </summary>
    public required string ItemId { get; init; }
    
    /// <summary>
    /// 装备名称
    /// </summary>
    public required string ItemName { get; init; }
    
    /// <summary>
    /// 优先级 (1-5，1最高)
    /// </summary>
    public required int Priority { get; init; }
    
    /// <summary>
    /// 胜率
    /// </summary>
    public double WinRate { get; init; }
    
    /// <summary>
    /// 前四率
    /// </summary>
    public double Top4Rate { get; init; }
    
    /// <summary>
    /// 登顶率
    /// </summary>
    public double Top1Rate { get; init; }
    
    /// <summary>
    /// 选择率
    /// </summary>
    public double PickRate { get; init; }
    
    /// <summary>
    /// 经济价值
    /// </summary>
    public double EconomicValue { get; init; }
}

/// <summary>
/// 装备优先级
/// </summary>
public sealed class ItemPriority
{
    /// <summary>
    /// 装备ID
    /// </summary>
    public required string ItemId { get; init; }
    
    /// <summary>
    /// 装备名称
    /// </summary>
    public required string ItemName { get; init; }
    
    /// <summary>
    /// 优先级分数 (0-100)
    /// </summary>
    public required double PriorityScore { get; init; }
    
    /// <summary>
    /// 适用英雄
    /// </summary>
    public List<string> SuitableChampions { get; init; } = new List<string>();
}
