namespace TFTAssistant.Core.Models.BigData;

// 装备优先级模型
public sealed class EquipmentPriority
{
    public string EquipmentId { get; init; } = "";
    public string EquipmentName { get; init; } = "";
    public double PriorityScore { get; init; }
    public double WinRate { get; init; }
    public double PickRate { get; init; }
    public int MatchCount { get; init; }
}

// 阵容克制模型
public sealed class LineupCounter
{
    public string LineupId { get; init; } = "";
    public string LineupName { get; init; } = "";
    public double CounterScore { get; init; }
    public double WinRate { get; init; }
    public double PickRate { get; init; }
}

// 克制分析结果模型
public sealed class CounterAnalysis
{
    public string LineupId { get; init; } = "";
    public IEnumerable<LineupCounter> Counters { get; init; } = Enumerable.Empty<LineupCounter>();
    public IEnumerable<LineupCounter> CounteredBy { get; init; } = Enumerable.Empty<LineupCounter>();
}
