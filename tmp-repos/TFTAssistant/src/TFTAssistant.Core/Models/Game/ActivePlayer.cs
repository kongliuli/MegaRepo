using System.Linq;

namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 当前活跃玩家
/// 包含玩家的详细信息，如等级、金币、生命值等
/// </summary>
public sealed class ActivePlayer : IEquatable<ActivePlayer>
{
    /// <summary>
    /// 玩家召唤师名称
    /// </summary>
    public required string SummonerName { get; init; }
    
    /// <summary>
    /// 玩家等级
    /// </summary>
    public required int Level { get; init; }
    
    /// <summary>
    /// 当前金币
    /// </summary>
    public required int CurrentGold { get; init; }
    
    /// <summary>
    /// 生命值
    /// </summary>
    public required double Health { get; init; }
    
    /// <summary>
    /// 经验值
    /// </summary>
    public required int Experience { get; init; }
    
    /// <summary>
    /// 总金币
    /// </summary>
    public required int TotalGold { get; init; }
    
    /// <summary>
    /// 当前排名
    /// </summary>
    public required int Placement { get; init; }
    
    /// <summary>
    /// 棋盘上的单位
    /// </summary>
    public required IReadOnlyList<BoardUnit> Board { get; init; }
    
    /// <summary>
    /// 备战席上的单位
    /// </summary>
    public required IReadOnlyList<BenchUnit> Bench { get; init; }
    
    /// <summary>
    /// 商店中的单位
    /// </summary>
    public required IReadOnlyList<ShopUnit> Shop { get; init; }
    
    /// <summary>
    /// 已选择的强化符文
    /// </summary>
    public IReadOnlyList<Augment> Augments { get; init; } = new List<Augment>();
    
    /// <summary>
    /// 玩家拥有的装备
    /// </summary>
    public IReadOnlyList<string> Items { get; init; } = new List<string>();
    
    /// <summary>
    /// 连胜次数
    /// </summary>
    public int WinStreak { get; init; } = 0;
    
    /// <summary>
    /// 连败次数
    /// </summary>
    public int LoseStreak { get; init; } = 0;

    /// <summary>
    /// 比较两个活跃玩家是否相等
    /// </summary>
    /// <param name="other">要比较的活跃玩家</param>
    /// <returns>是否相等</returns>
    public bool Equals(ActivePlayer? other)
    {
        if (other is null) return false;
        return Level == other.Level
            && CurrentGold == other.CurrentGold
            && Health == other.Health
            && Board.SequenceEqual(other.Board)
            && Bench.SequenceEqual(other.Bench);
    }

    /// <summary>
    /// 比较对象是否相等
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>是否相等</returns>
    public override bool Equals(object? obj) => Equals(obj as ActivePlayer);

    /// <summary>
    /// 获取哈希码
    /// </summary>
    /// <returns>哈希码</returns>
    public override int GetHashCode() => HashCode.Combine(Level, CurrentGold, Health);

    /// <summary>
    /// 验证活跃玩家信息是否有效
    /// </summary>
    /// <returns>是否有效</returns>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(SummonerName))
            return false;
        
        if (Level < 1 || Level > 9)
            return false;
        
        if (CurrentGold < 0 || TotalGold < 0)
            return false;
        
        if (Health < 0 || Health > 100)
            return false;
        
        if (Placement < 1 || Placement > 8)
            return false;
        
        if (Board == null || Bench == null || Shop == null)
            return false;
        
        // 验证所有单位数据
        foreach (var unit in Board.Concat(Bench.Cast<BoardUnit>()))
        {
            if (!unit.IsValid())
                return false;
        }
        
        return true;
    }
}
