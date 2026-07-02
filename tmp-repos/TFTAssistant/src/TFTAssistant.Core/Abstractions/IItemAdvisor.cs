using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 装备建议器接口
/// 提供基于大数据的装备分析和建议
/// </summary>
public interface IItemAdvisor
{
    /// <summary>
    /// 获取装备建议
    /// </summary>
    /// <param name="board">当前棋盘上的单位</param>
    /// <param name="availableComponents">可用的装备组件</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备建议列表</returns>
    Task<List<ItemSuggestion>> AdviseAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<ItemComponent> availableComponents,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 分析装备组合效果
    /// </summary>
    /// <param name="items">装备列表</param>
    /// <param name="championName">目标英雄</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>装备组合分析结果</returns>
    Task<ItemCombinationAnalysis> AnalyzeItemCombinationAsync(
        List<string> items,
        string championName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取阵容的装备选择建议
    /// </summary>
    /// <param name="lineupId">阵容ID</param>
    /// <param name="setVersion">版本</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>阵容装备建议</returns>
    Task<LineupItemRecommendation> GetLineupItemRecommendationAsync(
        string lineupId,
        string setVersion,
        CancellationToken cancellationToken = default);
}
