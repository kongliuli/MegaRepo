namespace TFTAssistant.Core.Models.Static;

public sealed class ItemComponent
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public ItemAcquisition Acquisition { get; init; } = ItemAcquisition.MonsterDrop;
}
