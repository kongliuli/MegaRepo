namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 游戏状态
/// 表示游戏的当前状态，包含玩家信息、游戏信息和强化符文等
/// </summary>
public sealed class GameState : IEquatable<GameState>
{
    /// <summary>
    /// 当前活跃玩家
    /// </summary>
    public required ActivePlayer ActivePlayer { get; init; }
    
    /// <summary>
    /// 所有玩家信息
    /// </summary>
    public required IReadOnlyList<PlayerSummary> AllPlayers { get; init; }
    
    /// <summary>
    /// 游戏基本信息
    /// </summary>
    public required GameInfo GameInfo { get; init; }
    
    /// <summary>
    /// 可用的强化符文
    /// </summary>
    public required IReadOnlyList<Augment> Augments { get; init; }
    
    /// <summary>
    /// 状态时间戳
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 比较两个游戏状态是否相等
    /// </summary>
    /// <param name="other">要比较的游戏状态</param>
    /// <returns>是否相等</returns>
    public bool Equals(GameState? other)
    {
        if (other is null) return false;
        return ActivePlayer.Equals(other.ActivePlayer)
            && GameInfo.Round == other.GameInfo.Round;
    }

    /// <summary>
    /// 比较对象是否相等
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>是否相等</returns>
    public override bool Equals(object? obj) => Equals(obj as GameState);

    /// <summary>
    /// 获取哈希码
    /// </summary>
    /// <returns>哈希码</returns>
    public override int GetHashCode() => HashCode.Combine(ActivePlayer, GameInfo.Round);

    /// <summary>
    /// 验证游戏状态是否有效
    /// </summary>
    /// <returns>是否有效</returns>
    public bool IsValid()
    {
        if (ActivePlayer == null)
            return false;
        
        if (!ActivePlayer.IsValid())
            return false;
        
        if (AllPlayers == null || AllPlayers.Count == 0)
            return false;
        
        if (GameInfo == null || !GameInfo.IsValid())
            return false;
        
        if (Augments == null)
            return false;
        
        return true;
    }
}
