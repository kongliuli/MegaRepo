using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 推荐引擎入口
/// 整合各种建议功能，提供综合推荐
/// </summary>
public interface IRecommendationEngine
{
    /// <summary>
    /// 获取综合推荐
    /// </summary>
    /// <param name="state">游戏状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>推荐集合</returns>
    Task<RecommendationSet> GetRecommendationsAsync(GameState state, CancellationToken cancellationToken = default);
}
