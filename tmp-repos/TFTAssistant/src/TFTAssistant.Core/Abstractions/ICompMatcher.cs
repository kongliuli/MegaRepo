using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 阵容匹配器接口
/// 根据当前游戏状态匹配最合适的阵容
/// </summary>
public interface ICompMatcher
{
    /// <summary>
    /// 匹配阵容
    /// </summary>
    /// <param name="board">棋盘上的单位</param>
    /// <param name="bench">备战席上的单位</param>
    /// <param name="augments">已选择的强化符文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>阵容建议列表</returns>
    Task<List<CompSuggestion>> MatchAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<BenchUnit> bench,
        IReadOnlyList<Augment> augments,
        CancellationToken cancellationToken = default);
}
