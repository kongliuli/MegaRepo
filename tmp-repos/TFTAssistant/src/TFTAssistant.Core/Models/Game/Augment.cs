namespace TFTAssistant.Core.Models.Game;

public sealed class Augment
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int Stage { get; init; }
}
