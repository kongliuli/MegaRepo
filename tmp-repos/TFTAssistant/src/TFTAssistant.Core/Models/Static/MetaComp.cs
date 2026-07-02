namespace TFTAssistant.Core.Models.Static;

public sealed class MetaComp
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public IReadOnlyList<string> Champions { get; init; } = new List<string>();
    public IReadOnlyList<string> Traits { get; init; } = new List<string>();
    public double WinRate { get; init; }
    public double PlayRate { get; init; }
}
