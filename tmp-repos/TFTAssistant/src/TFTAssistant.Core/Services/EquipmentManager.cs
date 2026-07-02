using System.Collections.Generic;
using System.Linq;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Services;

public sealed class EquipmentManager : IEquipmentManager
{
    private readonly IReadOnlyList<Item> _items;
    private readonly IReadOnlyList<ItemComponent> _components;
    private readonly Dictionary<string, Item> _itemById;
    private readonly Dictionary<string, ItemComponent> _componentById;
    private readonly Dictionary<ItemType, List<Item>> _itemsByType;
    private readonly Dictionary<ItemAcquisition, List<Item>> _itemsByAcquisition;
    private readonly Dictionary<string, List<Item>> _componentsToItems;

    public EquipmentManager(IStaticDataProvider staticDataProvider)
    {
        // 同步获取装备数据，确保在构造时就有完整的数据
        var itemsTask = staticDataProvider.GetItemsAsync("set13");
        itemsTask.Wait();
        _items = itemsTask.Result;
        
        // 假设 ItemComponent 是从 items 中过滤出来的
        _components = _items.Where(item => item.Type == ItemType.Component).ToList();
        
        _itemById = _items.ToDictionary(item => item.Id);
        _componentById = _components.ToDictionary(component => component.Id);
        _itemsByType = _items.GroupBy(item => item.Type).ToDictionary(group => group.Key, group => group.ToList());
        _itemsByAcquisition = _items.GroupBy(item => item.Acquisition).ToDictionary(group => group.Key, group => group.ToList());
        _componentsToItems = BuildComponentToItemsMap();
    }

    public IReadOnlyList<Item> GetAllItems() => _items;

    public IReadOnlyList<Item> GetItemsByType(ItemType type)
    {
        return _itemsByType.TryGetValue(type, out var items) ? items : new List<Item>();
    }

    public IReadOnlyList<Item> GetItemsByAcquisition(ItemAcquisition acquisition)
    {
        return _itemsByAcquisition.TryGetValue(acquisition, out var items) ? items : new List<Item>();
    }

    public IReadOnlyList<ItemComponent> GetAllComponents() => _components;

    public Item? GetItemById(string id)
    {
        return _itemById.TryGetValue(id, out var item) ? item : null;
    }

    public ItemComponent? GetComponentById(string id)
    {
        return _componentById.TryGetValue(id, out var component) ? component : null;
    }

    public IReadOnlyList<string> GetItemComponents(string itemId)
    {
        var item = GetItemById(itemId);
        return item?.Components ?? new List<string>();
    }

    public IReadOnlyList<Item> GetPossibleItemsFromComponents(IEnumerable<string> componentIds)
    {
        var componentIdSet = new HashSet<string>(componentIds);
        return _items.Where(item => 
            item.Type == ItemType.Completed || item.Type == ItemType.Radiant
        ).Where(item => 
            item.Components.All(componentId => componentIdSet.Contains(componentId))
        ).ToList();
    }

    public IReadOnlyList<Item> GetItemsFromComponent(string componentId)
    {
        return _componentsToItems.TryGetValue(componentId, out var items) ? items : new List<Item>();
    }

    public bool IsArtifact(string itemId)
    {
        var item = GetItemById(itemId);
        return item?.Type == ItemType.Artifact;
    }

    public bool IsRadiant(string itemId)
    {
        var item = GetItemById(itemId);
        return item?.Type == ItemType.Radiant;
    }

    public bool IsCompletedItem(string itemId)
    {
        var item = GetItemById(itemId);
        return item?.Type == ItemType.Completed;
    }

    public bool IsComponent(string itemId)
    {
        return _componentById.ContainsKey(itemId);
    }

    public ItemAcquisition GetItemAcquisition(string itemId)
    {
        var item = GetItemById(itemId);
        return item?.Acquisition ?? ItemAcquisition.Other;
    }

    public ItemAcquisition GetComponentAcquisition(string componentId)
    {
        var component = GetComponentById(componentId);
        return component?.Acquisition ?? ItemAcquisition.Other;
    }

    private Dictionary<string, List<Item>> BuildComponentToItemsMap()
    {
        var map = new Dictionary<string, List<Item>>();
        
        foreach (var item in _items)
        {
            if (item.Type == ItemType.Completed || item.Type == ItemType.Radiant)
            {
                foreach (var componentId in item.Components)
                {
                    if (!map.ContainsKey(componentId))
                    {
                        map[componentId] = new List<Item>();
                    }
                    map[componentId].Add(item);
                }
            }
        }
        
        return map;
    }
}