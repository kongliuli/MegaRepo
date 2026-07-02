# TFTAssistant 保留说明

日期：2026-07-02
仓库：`D:\Code\githubDown\TFTAssistant`

## 结论

暂不删除。

这是一个结构完整的 Teamfight Tactics 辅助工具项目，有核心库、Overwolf 桥接、桌面/覆盖层 UI、SQLite 存储和较完整测试。是否长期维护取决于你还做不做 TFT 方向，但它不属于“完全无用”的清理对象。

## 分支情况

| 分支 | 判断 |
| --- | --- |
| `origin/main` | 唯一远端主线，内容完整 |

## 项目结构

| 路径 | 价值 |
| --- | --- |
| `TFTAssistant.sln` | 解决方案入口 |
| `src/TFTAssistant.Core` | 推荐引擎、数据模型、服务层、SQLite 存储 |
| `src/TFTAssistant.Overwolf` | Overwolf 桥接层 |
| `src/TFTAssistant.App` | 应用入口 |
| `src/ui` | 桌面、覆盖层、background UI |
| `tests/TFTAssistant.Core.Tests` | 单元测试和集成测试 |
| `docs/compliance-review.md` | Riot 第三方工具合规审查 |
| `.trae/specs` | 需求和任务拆解 |

## 可复用点

- 推荐引擎：`RecommendationEngine`、`ItemAdvisor`、`CompMatcher`、`EconomyAdvisor`。
- 数据接入：`DataDragonProvider`、`LiveClientDataProvider`、`OverwolfEventAdapter`。
- 分析能力：装备、阵容、Meta、胜率统计。
- 合规材料：明确不做实时预测、对手棋盘侦察、动态操作指令等高风险功能。

## 后续处理

保留本仓。后续如果确认不再做 TFT 方向，再单独写删除前复活说明。

