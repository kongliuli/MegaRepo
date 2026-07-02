namespace TFTAssistant.Core.Models.Recommendation;

/// <summary>
/// 推荐集合
/// 整合各种建议功能的结果
/// </summary>
public sealed class RecommendationSet
{
    /// <summary>
    /// 阵容建议
    /// </summary>
    public required List<CompSuggestion> CompSuggestions { get; init; }
    
    /// <summary>
    /// 装备建议
    /// </summary>
    public required List<ItemSuggestion> ItemSuggestions { get; init; }
    
    /// <summary>
    /// 经济提示
    /// </summary>
    public EconomyHint? EconomyHint { get; init; }
    
    /// <summary>
    /// 强化符文评分
    /// </summary>
    public required List<AugmentRating> AugmentRatings { get; init; }
    
    /// <summary>
    /// 分析来源
    /// </summary>
    public string AnalysisSource { get; init; } = "历史统计数据";
}

/// <summary>
/// 阵容建议
/// </summary>
public sealed class CompSuggestion
{
    /// <summary>
    /// 阵容
    /// </summary>
    public required object Comp { get; init; }
    
    /// <summary>
    /// 评分
    /// </summary>
    public required double Score { get; init; }
    
    /// <summary>
    /// 已拥有的单位
    /// </summary>
    public required List<string> OwnedUnits { get; init; }
    
    /// <summary>
    /// 缺失的单位
    /// </summary>
    public required List<string> MissingUnits { get; init; }
    
    /// <summary>
    /// 羁绊覆盖率
    /// </summary>
    public required double TraitCoverage { get; init; }
    
    /// <summary>
    /// 推荐理由
    /// </summary>
    public string Reasoning { get; init; } = string.Empty;
}

/// <summary>
/// 经济提示
/// </summary>
public sealed class EconomyHint
{
    /// <summary>
    /// 标题
    /// </summary>
    public required string Title { get; init; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public required string Description { get; init; }
    
    /// <summary>
    /// 紧急程度
    /// </summary>
    public required HintUrgency Urgency { get; init; }
    
    /// <summary>
    /// 推荐理由
    /// </summary>
    public string Reasoning { get; init; } = string.Empty;
}

/// <summary>
/// 提示紧急程度
/// </summary>
public enum HintUrgency
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// 强化符文评分
/// </summary>
public sealed class AugmentRating
{
    /// <summary>
    /// 强化符文ID
    /// </summary>
    public required string AugmentId { get; init; }
    
    /// <summary>
    /// 强化符文名称
    /// </summary>
    public required string AugmentName { get; init; }
    
    /// <summary>
    /// 评分 (0-100)
    /// </summary>
    public required double Score { get; init; }
    
    /// <summary>
    /// 分析来源
    /// </summary>
    public string AnalysisSource { get; init; } = "历史统计数据";
    
    /// <summary>
    /// 推荐理由
    /// </summary>
    public string Reasoning { get; init; } = string.Empty;
}
