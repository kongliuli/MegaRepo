namespace TFTAssistant.Core.Models.Static;

public enum ItemAcquisition
{
    // 基础散件 - 从野怪掉落
    MonsterDrop,
    // 成装 - 通过散件合成
    Crafting,
    // 光明装备 - 从光明武器库获取
    RadiantChest,
    // 神器 - 从神器库或特殊事件获取
    ArtifactChest,
    // 其他获取方式
    Other
}