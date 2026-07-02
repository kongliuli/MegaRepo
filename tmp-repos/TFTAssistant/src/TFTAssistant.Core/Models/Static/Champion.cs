namespace TFTAssistant.Core.Models.Static;

public sealed class Champion
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public int Cost { get; init; }
    public IReadOnlyList<string> Traits { get; init; } = new List<string>();
    public string ImageUrl { get; init; } = "";
}
