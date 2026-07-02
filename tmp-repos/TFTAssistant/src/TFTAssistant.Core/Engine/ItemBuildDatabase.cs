using TFTAssistant.Core.Models.Static;
using System.Collections.Frozen;

namespace TFTAssistant.Core.Engine;

/// <summary>
/// 装备推荐规则表（静态数据，来自社区经验）
/// 每个英雄对应其最佳装备列表
/// </summary>
public static class ItemBuildDatabase
{
    private static readonly FrozenDictionary<string, List<Item>> _builds = 
        new Dictionary<string, List<Item>>
        {
            // 示例数据，需根据当前赛季更新
            ["Ahri"] = new List<Item>
            {
                new() { Id = "Shojin", Name = "Spear of Shojin", Components = new() { "Tear", "Sword" }, IsComponent = false },
                new() { Id = "BlueBuff", Name = "Blue Buff", Components = new() { "Tear", "Tear" }, IsComponent = false },
                new() { Id = "JeweledGauntlet", Name = "Jeweled Gauntlet", Components = new() { "Glove", "Rod" }, IsComponent = false }
            },
            ["Akali"] = new List<Item>
            {
                new() { Id = "InfinityEdge", Name = "Infinity Edge", Components = new() { "Sword", "Cloak" }, IsComponent = false },
                new() { Id = "GuinsoosRageblade", Name = "Guinsoo's Rageblade", Components = new() { "Bow", "Rod" }, IsComponent = false },
                new() { Id = "Bloodthirster", Name = "Bloodthirster", Components = new() { "Sword", "Vamp" }, IsComponent = false }
            },
            ["Ashe"] = new List<Item>
            {
                new() { Id = "GiantSlayer", Name = "Giant Slayer", Components = new() { "Bow", "Sword" }, IsComponent = false },
                new() { Id = "RapidFirecannon", Name = "Rapid Firecannon", Components = new() { "Bow", "Bow" }, IsComponent = false },
                new() { Id = "GuardianAngel", Name = "Guardian Angel", Components = new() { "Sword", "Belt" }, IsComponent = false }
            },
            ["Draven"] = new List<Item>
            {
                new() { Id = "InfinityEdge", Name = "Infinity Edge", Components = new() { "Sword", "Cloak" }, IsComponent = false },
                new() { Id = "Bloodthirster", Name = "Bloodthirster", Components = new() { "Sword", "Vamp" }, IsComponent = false },
                new() { Id = "LastWhisper", Name = "Last Whisper", Components = new() { "Bow", "Cloak" }, IsComponent = false }
            },
            ["Kai'Sa"] = new List<Item>
            {
                new() { Id = "GuinsoosRageblade", Name = "Guinsoo's Rageblade", Components = new() { "Bow", "Rod" }, IsComponent = false },
                new() { Id = "RapidFirecannon", Name = "Rapid Firecannon", Components = new() { "Bow", "Bow" }, IsComponent = false },
                new() { Id = "GiantSlayer", Name = "Giant Slayer", Components = new() { "Bow", "Sword" }, IsComponent = false }
            },
            ["Leona"] = new List<Item>
            {
                new() { Id = "SunfireCape", Name = "Sunfire Cape", Components = new() { "Belt", "Belt" }, IsComponent = false },
                new() { Id = "WarmogsArmor", Name = "Warmog's Armor", Components = new() { "Belt", "Belt" }, IsComponent = false },
                new() { Id = "BrambleVest", Name = "Bramble Vest", Components = new() { "Chain", "Chain" }, IsComponent = false }
            },
            ["Lux"] = new List<Item>
            {
                new() { Id = "Morellonomicon", Name = "Morellonomicon", Components = new() { "Rod", "Belt" }, IsComponent = false },
                new() { Id = "BlueBuff", Name = "Blue Buff", Components = new() { "Tear", "Tear" }, IsComponent = false },
                new() { Id = "Zhonya's Hourglass", Name = "Zhonya's Hourglass", Components = new() { "Cloak", "Rod" }, IsComponent = false }
            },
            ["MissFortune"] = new List<Item>
            {
                new() { Id = "GiantSlayer", Name = "Giant Slayer", Components = new() { "Bow", "Sword" }, IsComponent = false },
                new() { Id = "RapidFirecannon", Name = "Rapid Firecannon", Components = new() { "Bow", "Bow" }, IsComponent = false },
                new() { Id = "InfinityEdge", Name = "Infinity Edge", Components = new() { "Sword", "Cloak" }, IsComponent = false }
            },
            ["Sett"] = new List<Item>
            {
                new() { Id = "TitanicHydra", Name = "Titanic Hydra", Components = new() { "Belt", "Sword" }, IsComponent = false },
                new() { Id = "WarmogsArmor", Name = "Warmog's Armor", Components = new() { "Belt", "Belt" }, IsComponent = false },
                new() { Id = "GuardianAngel", Name = "Guardian Angel", Components = new() { "Sword", "Belt" }, IsComponent = false }
            },
            ["Syndra"] = new List<Item>
            {
                new() { Id = "Morellonomicon", Name = "Morellonomicon", Components = new() { "Rod", "Belt" }, IsComponent = false },
                new() { Id = "BlueBuff", Name = "Blue Buff", Components = new() { "Tear", "Tear" }, IsComponent = false },
                new() { Id = "VoidStaff", Name = "Void Staff", Components = new() { "Rod", "Rod" }, IsComponent = false }
            }
        }.ToFrozenDictionary();

    public static List<Item> Get(string championName)
    {
        return _builds.GetValueOrDefault(championName, new List<Item>());
    }

    public static void Update(string championName, List<Item> builds)
    {
        // 支持运行时更新（从配置文件加载）
        // 实际实现需要使用可变字典
    }

    public static IReadOnlyList<string> GetAllChampions()
    {
        return _builds.Keys.ToList();
    }
}
