namespace TFTAssistant.Core.Models.Static;

public sealed class Item
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public IReadOnlyList<string> Components { get; init; } = new List<string>();
    public string ImageUrl { get; init; } = "";
    public ItemType Type { get; init; } = ItemType.Completed;
    public ItemAcquisition Acquisition { get; init; } = ItemAcquisition.Crafting;
}
