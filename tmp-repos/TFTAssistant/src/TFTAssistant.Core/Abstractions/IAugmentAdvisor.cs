using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 强化符文顾问接口
/// 对强化符文进行评分和建议
/// </summary>
public interface IAugmentAdvisor
{
    /// <summary>
    /// 对强化符文进行评分
    /// </summary>
    /// <param name="options">可用的强化符文选项</param>
    /// <param name="existingAugments">已选择的强化符文</param>
    /// <param name="boardUnits">棋盘上的单位</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>强化符文评分列表</returns>
    Task<List<AugmentRating>> RateAsync(
        IReadOnlyList<Augment> options,
        IReadOnlyList<Augment> existingAugments,
        IReadOnlyList<BoardUnit> boardUnits,
        CancellationToken cancellationToken = default);
}
