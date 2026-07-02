# MegaRepo 分支逐项分析

日期：2026-07-02
仓库：`D:\Code\githubDown\MegaRepo`

## 总结

`MegaRepo` 的 `main` 不是代码主线，而是分支索引和归档说明。真正代码分散在 `archive/*` 和 `feature/*` 分支。

`archive/localdts` 曾是最值得拆的分支，但现在已经确认存在独立私有仓 `LocalDts`，后续以私有仓为准。

2026-07-02 已执行清理：在 `MegaRepo` 新增并推送 `archive/revival-docs` 分支保存本轮复活文档；随后删除已留档的旧远端分支。当前 GitHub 远端仅保留：

- `main`
- `archive/revival-docs`

三个此前标记“再看一眼”的分支已复查：

- `feature/outpatient-inventory`：早期骨架，可删。
- `feature/programcell`：只留 GitLab/Jenkins 看板想法，可删。
- `feature/yunwei-tool`：只留项目/环境模型想法，可删。

其余分支大多是重复、旧工具、只有文档，或含大量生成物。

## 分支清单

| 分支 | 文件数 | 项目数 | 生成物 | 结论 |
| --- | ---: | ---: | ---: | --- |
| `origin/main` | 36 | 0 | 0 | 保留为索引即可，不是代码主线 |
| `origin/trae/solo-agent-edXDXQ` | 20 | 0 | 0 | 删除或忽略，只有整理过程文件 |
| `origin/archive/localdts` | 402 | 38 | 0 | 已被私有仓 `LocalDts` 覆盖，不再从 MegaRepo 复活 |
| `origin/archive/mindmap` | 24 | 0 | 0 | 只归档，不值得单独维护 |
| `origin/feature/autoticket` | 48 | 2 | 0 | 删除或归档 |
| `origin/feature/autowebclient` | 225 | 16 | 0 | 删除或归档 |
| `origin/feature/blazor-trae` | 141 | 2 | 80 | 删除或归档，含大量 `obj` |
| `origin/feature/dbconnection-wpf` | 214 | 2 | 167 | 删除或归档，且此前已不保留 |
| `origin/feature/dingtalklib` | 27 | 2 | 0 | 重复，已在 `2025CodeRepository` 留档 |
| `origin/feature/diting-utils` | 32 | 2 | 0 | 删除或归档 |
| `origin/feature/fileexplorer` | 402 | 2 | 354 | 重复，已在 `2025CodeRepository` 留档，且含大量生成物 |
| `origin/feature/outpatient-inventory` | 74 | 6 | 0 | 可删，只留文档记录 |
| `origin/feature/programcell` | 60 | 1 | 0 | 可删，只留 GitLab/Jenkins 看板想法 |
| `origin/feature/tools-components` | 120 | 4 | 69 | 删除或归档，含生成物 |
| `origin/feature/universal-invoice` | 21 | 0 | 0 | 删除，只有文档 |
| `origin/feature/yunwei-tool` | 133 | 2 | 45 | 可删，只留项目/环境模型想法 |

## 逐分支说明

### `origin/main`

用途：`MegaRepo` 索引分支。

内容：`README.md`、`docs/branches`、`analysis`、`tmp-repos` 等文档和整理记录。

结论：保留也可以，但它不是代码主线。后续如果拆完有价值分支，可以把这个仓库归档。

### `origin/trae/solo-agent-edXDXQ`

用途：AI 整理过程分支。

内容：20 个文件，无项目文件，主要是 `tmp-repos`、`analysis`、`complete-repos.json`、`BRANCH.md`。

结论：删除或忽略。没有长期保留价值。

### `origin/archive/localdts`

用途：LocalDts 数据迁移工具归档。

内容：38 个项目，包含：

- `DataMigration.Contracts`
- `DataMigration.Core`
- `DataMigration.Console`
- `DataMigration.Wpf`
- `DataMigration.AutomatedTests`
- 多个插件项目，如 Azure Blob、CSV、Excel、MySQL、SQLite、SQL Server、JSON、XML、Transformer 等

价值：这是 `MegaRepo` 中最像完整产品/平台的分支。功能面完整，代码量和模块边界都够独立。

结论：不用再从 `MegaRepo` 拆。独立私有仓 `LocalDts` 已存在，且 `origin/trae/solo-agent-v64RLs` 还有 checkpoint/security/tests 增强，后续以 `LocalDts` 私有仓为准。

### `origin/archive/mindmap`

用途：个人思维导图/大纲工具归档。

内容：24 个文件，无项目文件，主要是文档和少量归档内容。

结论：只做归档即可，不建议单独维护。

### `origin/feature/autoticket`

用途：自动购票工具。

内容：`.NET 5`、WinForms、HTTP 封装、代理、验证码、JS 签名等。

判断：技术和业务风险都高，后续复用价值低。

结论：删除或归档。

### `origin/feature/autowebclient`

用途：CEFSharp 自动化 Web 客户端集合。

内容：16 个项目，包含阿里云自动化、支付宝订单抓取、自动购票、CEFSharp 控件等。

判断：项目多但方向偏旧，且自动化抓取/购票类内容维护成本高。

结论：删除或归档，不建议继续作为独立方向维护。

### `origin/feature/blazor-trae`

用途：Blazor demo。

内容：地图标注、天气、todo 等页面，2 个项目文件。

问题：分支里有 80 个生成物，主要是 `obj`。

结论：删除或归档。若要留，只留源码并清掉生成物。

### `origin/feature/dbconnection-wpf`

用途：WPF 数据库连接管理工具。

内容：2 个项目文件，`Models`、`Utils`、`Views` 等。

问题：167 个生成物，且此前你已在 `2025CodeRepository` 清理时决定不保留。

结论：删除或归档。

### `origin/feature/dingtalklib`

用途：钉钉接口封装库。

内容：`DingTalkClient`、消息内容、@ 设置、测试样例。

判断：有参考价值，但已经在 `2025CodeRepository` 留档。

结论：重复分支，不需要在 `MegaRepo` 再保留一份。

### `origin/feature/diting-utils`

用途：通用工具库。

内容：数据处理、INI、JSON、XML、接口抽象等。

判断：典型“通用 utils”仓，独特价值低，后续容易变成没人敢删的杂物。

结论：删除或归档。

### `origin/feature/fileexplorer`

用途：WPF/MVVM 文件浏览器样例。

内容：2 个项目文件，源码本身有参考价值。

问题：354 个生成物，且源码已经在 `2025CodeRepository` 留档。

结论：重复分支，不需要在 `MegaRepo` 再保留一份。

### `origin/feature/outpatient-inventory`

用途：门诊库存管理系统。

内容：6 个项目，包含：

- CodeFirst 数据层
- MAUI 客户端
- Models
- Service
- WebAPI

判断：小但真实，有业务形态，但代码仍是早期骨架；`ItemInfoController.SelectItemInfo` 当前调用查询后直接返回新的空 `ItemInfomation`。

结论：可删。若以后需要门诊库存，只参考模型/分层思路，不恢复整分支。

### `origin/feature/programcell`

用途：Blazor 项目管理工具。

内容：1 个项目，包含 GitLab、JenkinsJob、项目卡片、Ant Design UI 等。

判断：可能和运维平台、项目看板类需求重叠，但实现很薄，主要是 GitLab 域名/物料和 Jenkins job 的展示页面。

结论：可删。保留想法即可，后续在 `yf.pt` 里重写比恢复整分支更便宜。

### `origin/feature/tools-components`

用途：Windows Forms 小工具集合。

内容：`NewTextByCard`、`SuffixChange.Form` 等。

问题：69 个生成物。

结论：删除或归档。除非某个小工具现在还在用，否则不值得单独保留。

### `origin/feature/universal-invoice`

用途：通用票据工具设想。

内容：只有文档和 README，无项目文件。

结论：删除。

### `origin/feature/yunwei-tool`

用途：Blazor Server 运维工具平台。

内容：2 个项目文件，包含项目管理、项目分组、运维操作页面。

问题：45 个生成物。

判断：和 `yf.pt` 的信息流/资源/告警/成本平台方向可能重叠；分支包含 `.vs`、`bin`、`obj` 生成物，代码价值集中在 `ProjectInfo` / `EnvironmentConfig` 这类模型。

结论：可删。若 `yf.pt` 后续需要项目/环境配置，只参考模型字段，不恢复整分支。

## 推荐动作

1. `LocalDts` 后续以独立私有仓为准，不再从 `MegaRepo/archive/localdts` 复活。
2. 旧远端分支已删除；需要恢复时从 `archive/revival-docs` 文档或本地 bundle 查 tip。
3. 如果保留 `MegaRepo`，让 `main` 只当索引，不再继续塞代码。
