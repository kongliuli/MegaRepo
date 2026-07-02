namespace TFTAssistant.Core.Abstractions;

using TFTAssistant.Core.Models.Recommendation;

/// <summary>
/// 经济顾问接口
/// 提供经济策略建议
/// </summary>
public interface IEconomyAdvisor
{
    /// <summary>
    /// 获取经济提示
    /// </summary>
    /// <param name="gold">当前金币</param>
    /// <param name="level">当前等级</param>
    /// <param name="health">当前生命值</param>
    /// <param name="winStreak">是否处于连胜</param>
    /// <param name="loseStreak">是否处于连败</param>
    /// <param name="stage">当前阶段</param>
    /// <returns>经济提示</returns>
    EconomyHint Advise(int gold, int level, int health, bool winStreak, bool loseStreak, int stage);
}
