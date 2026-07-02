# MegaRepo 候选分支复活说明

日期：2026-07-02
仓库：`D:\Code\githubDown\MegaRepo`

## 结论

`MegaRepo` 不是一个需要继续扩展的主仓。它的价值在分支里。

现在不建议从 `MegaRepo` 整分支复活任何代码。

原因：

- `archive/localdts` 已被独立私有仓 `LocalDts` 覆盖。
- `feature/outpatient-inventory`、`feature/programcell`、`feature/yunwei-tool` 已复查，只适合留想法，不适合恢复整分支。

其余分支按 `MEGAREPO_BRANCH_AUDIT.zh-CN.md` 的结论处理即可。

2026-07-02 已新增并推送 `archive/revival-docs` 分支保存本轮复活文档；`MegaRepo` 旧 `archive/*`、`feature/*`、`trae/*` 远端分支已清理。删除前完整备份在：

`D:\Code\githubDown\_github-delete-backups\MegaRepo-before-branch-cleanup-20260702.bundle`

## `archive/localdts`

结论：不再从 `MegaRepo` 复活。独立私有仓 `LocalDts` 已存在，后续以 `LocalDts` 为准。

定位：.NET 数据迁移工具，支持多数据源、多目标源、插件化转换、WPF/Console/Blazor 多入口。

项目规模：

- 402 个文件
- 38 个项目
- 无明显 `bin/obj` 生成物

核心项目：

| 路径 | 用途 |
| --- | --- |
| `archive/LocalDts/DataMigration.Contracts` | 契约层，定义数据源、目标、插件、转换器、迁移任务 |
| `archive/LocalDts/DataMigration.Core` | 核心迁移引擎、插件管理、日志、数据库/CSV/Excel helper |
| `archive/LocalDts/DataMigration.Console` | 命令行入口 |
| `archive/LocalDts/DataMigration.Wpf` | WPF 桌面入口 |
| `archive/LocalDts/WebDts.Blazor` | Blazor Web 入口 |
| `archive/LocalDts/DataMigration.AutomatedTests` | 自动化测试/验证入口 |
| `archive/LocalDts/Plugins/*` | CSV、Excel、MySQL、SQL Server、SQLite、RulesEngine 等插件 |
| `archive/LocalDts/DataMigration.Plugin.*` | Kafka、Mongo、Redis、REST API、Azure Blob、Elasticsearch 等扩展插件 |

可复用点：

- 插件契约：`IDataSource`、`IDataTarget`、`ITransformer`、`IPlugin`
- 迁移任务模型：`MigrationTask`
- 核心引擎：`MigrationEngine`、`MigrationService`
- 插件管理：`PluginManager`
- 数据源/目标插件结构
- WPF/Console/Blazor 多入口组织方式

复活路径：直接使用私有仓 `LocalDts`，并参考 `LOCALDTS_RETENTION_NOTE.zh-CN.md`。

## `feature/outpatient-inventory`

结论：可删，只留想法。

定位：门诊库存管理系统，包含 CodeFirst 数据层、MAUI 客户端、服务层、WebAPI。

项目规模：

- 74 个文件
- 6 个项目
- 无明显生成物

核心项目：

| 路径 | 用途 |
| --- | --- |
| `projects/outpatient-inventory/CodeFirst` | EF CodeFirst 数据模型 |
| `projects/outpatient-inventory/OutpatientInventoryManager.Maui` | MAUI 客户端 |
| `projects/outpatient-inventory/OutpatientInventoryManager.Models` | 模型层 |
| `projects/outpatient-inventory/OutpatientInventoryManager.Service` | 服务接口 |
| `projects/outpatient-inventory/OutpatientInventoryManager.WebAPI` | Web API |

可复用点：

- 门诊库存领域模型
- MAUI + WebAPI + Service 的小型分层结构
- `ItemInfoController`
- `DataShowViewModel`

不保留理由：

- 目前没有明确活跃业务线。
- 功能看起来仍偏早期。
- `ItemInfoController.SelectItemInfo` 当前返回空对象，业务闭环未完成。
- 如果未来医疗方向继续，`xinglin` / `ReportPlatform` 更像主线。

复活路径：

如果要复活，只抽 `OutpatientInventoryManager.Models`、`Service`、`WebAPI`，MAUI 客户端最后再考虑。

## `feature/programcell`

结论：想法可留，分支本体可删。

定位：Blazor 项目管理/看板工具，展示 GitLab、Jenkins、项目卡片。

项目规模：

- 60 个文件
- 1 个项目
- 无明显生成物

核心路径：

| 路径 | 用途 |
| --- | --- |
| `projects/programcell/Pages/Gitlab.razor` | GitLab 项目展示 |
| `projects/programcell/Pages/JenkinsJob.razor` | Jenkins 任务展示 |
| `projects/programcell/Data/GitLabModel.cs` | GitLab 数据模型 |
| `projects/programcell/Data/JenkinProject.cs` | Jenkins 项目模型 |
| `projects/programcell/Data/ProjectCardDataView.cs` | 项目卡片视图模型 |

可复用点：

- GitLab/Jenkins 项目聚合思路
- 项目卡片视图
- Ant Design + Blazor 管理界面结构

不保留理由：

- 与 `yf.pt` 的信息流/资源/告警平台方向可能重叠。
- 功能点少，适合提想法，不适合保留整个仓。

复活路径：

如果 `yf.pt` 后续要做项目/CI 面板，直接按页面和模型重写，不整分支恢复。

## `feature/yunwei-tool`

结论：只留项目/环境模型想法，分支本体可删。

定位：Blazor Server 运维工具平台，包含项目管理、项目分组、运维操作页面。

项目规模：

- 133 个文件
- 2 个项目
- 45 个生成物

核心路径：

| 路径 | 用途 |
| --- | --- |
| `projects/yunwei-tool/YunweiTool.sln` | 解决方案 |
| `projects/yunwei-tool/YunweiTool/YunweiTool.Server` | Blazor Server 主项目 |
| `Components/Pages/ProjectGroup.razor` | 项目分组页面 |
| `Components/Pages/ProjectInfomation.razor` | 项目信息页面 |
| `Data/Models/ProjectInfo.cs` | 项目信息模型 |
| `Data/Services/ProjecGroupService.cs` | 项目分组服务 |

可复用点：

- 运维项目分组模型
- 项目信息管理页面结构
- Blazor Server 管理后台壳

不保留理由：

- 生成物较多。
- 与 `yf.pt` 重叠。
- 代码量不大，重写比恢复整分支更便宜。

复活路径：

如果后续要进 `yf.pt`，只参考页面结构和模型字段，不恢复整个分支。

## 最小保留建议

要删 `MegaRepo` 前，至少保留这些文档：

- `MEGAREPO_BRANCH_AUDIT.zh-CN.md`
- `MEGAREPO_REVIVAL_NOTES.zh-CN.md`

真要复活代码，优先从独立仓取，不从 `MegaRepo` 分支取。

如果必须恢复已删除的 `MegaRepo` 远端分支，先从 bundle 还原，再按 `MEGAREPO_BRANCH_TIPS_BEFORE_DELETE.zh-CN.md` 中记录的 tip 建分支。
