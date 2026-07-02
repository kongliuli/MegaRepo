namespace TFTAssistant.Core.Models.Static;

public sealed class SetVersion
{
    public string Version { get; init; } = "";
    public string SetNumber { get; init; } = "";
    public DateTime ReleaseDate { get; init; }
    public bool IsLatest { get; init; }
}
