namespace TFTAssistant.Core.Models.BigData;

public sealed class EquipmentData
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string SetVersion { get; init; } = "";
    public string Description { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public bool IsComponent { get; init; }
    public IReadOnlyList<string> Components { get; init; } = new List<string>();
    public double WinRate { get; init; }
    public double Top4Rate { get; init; }
    public double Top1Rate { get; init; }
    public double PickRate { get; init; }
    public double EconomicValue { get; init; }
    public string EquipmentType { get; init; } = "";
    public int MatchCount { get; init; }
    public DateTime LastUpdated { get; init; }
}
