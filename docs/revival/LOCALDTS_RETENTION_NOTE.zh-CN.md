# LocalDts 保留说明

日期：2026-07-02
仓库：`D:\Code\githubDown\LocalDts`

## 结论

保留。

这是独立的数据迁移工具仓库，价值高于 `MegaRepo/archive/localdts`。后续如果要恢复或继续开发，应以这个私有仓为准，不再从 `MegaRepo` 里拆旧分支。

## 分支情况

| 分支 | 判断 |
| --- | --- |
| `origin/main` | 当前主线，包含 WPF、Console、Core、Contracts、插件和 WebDts.Blazor |
| `origin/trae/solo-agent-v64RLs` | 有有效增强，值得后续合并或摘取 |

## 核心结构

| 路径 | 价值 |
| --- | --- |
| `DataMigration.slnx` | 解决方案入口 |
| `DataMigration.Contracts` | 插件接口、迁移任务、数据记录等契约 |
| `DataMigration.Core` | 迁移引擎、插件管理、数据库 helper、日志和错误处理 |
| `DataMigration.Wpf` | WPF 图形界面 |
| `DataMigration.Console` | 命令行入口 |
| `Plugins` | CSV、Excel、MySQL、SQL Server、SQLite、转换器等插件 |
| `WebDts.Blazor` | Web 版迁移任务管理 |
| `DataMigration.Tests` | 核心测试 |
| `DataMigration.AutomatedTests` | 自动化迁移测试和样例数据 |

## `trae` 分支可摘取点

`origin/trae/solo-agent-v64RLs` 主要增加：

- `ICheckpointManager` / `CheckpointManager`：迁移 checkpoint。
- `IDataValidator` / `DataValidator`：数据校验。
- `SecurityHelper`：安全辅助。
- `MigrationExecutionService`：WebDts 执行服务。
- `CheckpointManagerTests` / `SecurityHelperTests`：新增测试。
- 插件目录整理：大量插件移动到统一 `Plugins/` 下。

## 后续处理

保留仓库。后续整理时优先比较并合并 `origin/trae/solo-agent-v64RLs`，再考虑清理已提交的插件 DLL / PDB 等生成物。

