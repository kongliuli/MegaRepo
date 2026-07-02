using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Abstractions;

public interface IEquipmentManager
{
    // 获取所有装备
    IReadOnlyList<Item> GetAllItems();
    
    // 根据类型获取装备
    IReadOnlyList<Item> GetItemsByType(ItemType type);
    
    // 根据获取方式获取装备
    IReadOnlyList<Item> GetItemsByAcquisition(ItemAcquisition acquisition);
    
    // 获取所有散件
    IReadOnlyList<ItemComponent> GetAllComponents();
    
    // 根据ID获取装备
    Item? GetItemById(string id);
    
    // 根据ID获取散件
    ItemComponent? GetComponentById(string id);
    
    // 获取成装的合成配方
    IReadOnlyList<string> GetItemComponents(string itemId);
    
    // 获取可以合成的成装
    IReadOnlyList<Item> GetPossibleItemsFromComponents(IEnumerable<string> componentIds);
    
    // 获取散件可以合成的成装
    IReadOnlyList<Item> GetItemsFromComponent(string componentId);
    
    // 检查装备是否为神器
    bool IsArtifact(string itemId);
    
    // 检查装备是否为光明装备
    bool IsRadiant(string itemId);
    
    // 检查装备是否为成装
    bool IsCompletedItem(string itemId);
    
    // 检查装备是否为散件
    bool IsComponent(string itemId);
    
    // 获取装备的获取方式
    ItemAcquisition GetItemAcquisition(string itemId);
    
    // 获取散件的获取方式
    ItemAcquisition GetComponentAcquisition(string componentId);
}