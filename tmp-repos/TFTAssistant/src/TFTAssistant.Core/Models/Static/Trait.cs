namespace TFTAssistant.Core.Models.Static;

public sealed class Trait
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public IReadOnlyList<int> Breakpoints { get; init; } = new List<int>();
    public string ImageUrl { get; init; } = "";
}
