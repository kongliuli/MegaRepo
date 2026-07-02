namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 游戏基本信息
/// 包含游戏时间、回合、阶段等基本信息
/// </summary>
public sealed class GameInfo
{
    /// <summary>
    /// 游戏时间（秒）
    /// </summary>
    public required double GameTime { get; init; }
    
    /// <summary>
    /// 当前回合数
    /// </summary>
    public required int Round { get; init; }
    
    /// <summary>
    /// 当前游戏阶段
    /// </summary>
    public required int Stage { get; init; }
    
    /// <summary>
    /// 是否为玩家对战
    /// </summary>
    public required bool IsPvp { get; init; }
    
    /// <summary>
    /// 游戏版本号
    /// </summary>
    public required string SetNumber { get; init; }

    /// <summary>
    /// 验证游戏信息是否有效
    /// </summary>
    /// <returns>是否有效</returns>
    public bool IsValid()
    {
        if (GameTime < 0)
            return false;
        
        if (Round < 1)
            return false;
        
        if (Stage < 1 || Stage > 9)
            return false;
        
        if (string.IsNullOrEmpty(SetNumber))
            return false;
        
        return true;
    }
}
