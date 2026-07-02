# xinglin-core 保留说明

日期：2026-07-02
仓库：`D:\Code\githubDown\xinglin-core`

## 结论

保留。

这是杏林相关核心能力仓，包含报告模板、动态录入、PDF 预览、数据适配、授权和测试。它和 `xinglin` / `ReportPlatform` 关系密切，不应进入删除候选。

## 分支情况

| 分支 | 判断 |
| --- | --- |
| `origin/master` | 唯一远端主线，保留 |

## 当前问题

- 未看到 `.sln` / `.slnx` 解决方案入口。
- README 提到 `docs/01_项目整体设计.md` 等文档，但当前分支未看到对应 `docs/` 文件。

这不是删除理由，只是后续整理时要补齐入口和文档索引。

## 核心结构

| 路径 | 价值 |
| --- | --- |
| `src/xinglin-core` | WPF 控件、视图、ViewModel、模板编辑和数据录入核心 |
| `src/Models` | PatientInfo、LabReportData 等模型 |
| `src/DataAdapters` | HIS、身份证读卡器、API 数据适配 |
| `src/LicenseLib` | 机器码、授权验证、授权配置 |
| `src/LicenseConsole` | 授权命令行工具 |
| `src/xinglin-core.UnitTests` | 服务和 ViewModel 测试 |
| `src/xinglin-core.Tests.UI` | UI 测试 |
| `src/xinglin-core.Tests.Integration` | 集成测试 |

## 可复用点

- 报告模板和控件箱。
- 动态数据录入 ViewModel。
- PDF 预览与服务测试。
- 条件规则、绑定路径、单位格式化。
- 多源病人数据适配。
- 本地授权机制。

## 后续处理

保留仓库。后续整理的最小动作是补一个 `.slnx`，再修 README 中不存在的 `docs/` 链接。

