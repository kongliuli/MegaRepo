# 16 个当前 GitHub 仓库完整分支处置表

日期：2026-07-02
账号：`kongliuli`

## 范围

当前 GitHub 仍可见 16 个仓库，其中 6 个是私有仓：

- 私有仓：`SpaceSniffMax`、`LocalDts`、`2025CodeRepository`、`TFTAssistant`、`xinglin-core`、`xinlingMain`
- 已删除远端但本地保留的仓库不计入这 16 个，单独列在末尾。

判断标准：

- 有实现：分支里有源码/项目文件/可继续开发的业务代码。
- 可删分支：已被主线吸收、纯过程文档、无差异、或已有更权威分支覆盖。
- 不删仓库：活跃项目、有非默认分支价值、或后续还会本地开发。

## 总览

| 仓库 | 可见性 | 仓库处理 | 分支处理 |
| --- | --- | --- | --- |
| `MegaRepo` | public | 保留为复活文档仓 | 只保留 `main`、`archive/revival-docs` |
| `PITS-Personal-Itinerary-Tracking-System` | public | 保留 | 后续入口是 `codex/merge-pits-branches`；两个 `trae` 可在确认后删 |
| `SpaceSniffMax` | private | 保留 | `master` 是真实主线；`main` 过薄，需要纠偏 |
| `Jvedio` | public | 保留 | `dev-5.0`、`kvdeio` 都有实现，先都留 |
| `SSSSR` | public | 保留 | 只有 `main` |
| `yf.pt` | public | 保留 | 本地已合并 `trae`，推送后可删 `trae` |
| `Feishu.Context` | public | 保留 | 本地已合并 `trae`，推送后可删 `trae` |
| `DataForge.Core` | public | 保留 | `trae` 已被 `main` 吸收，可删 |
| `ReportPlatform` | public | 活跃项目，保留 | `0520_master`、`trae` 都先留 |
| `MediaManager` | public | 活跃项目，保留 | `trae` 有 API/性能实现，先留 |
| `LocalDts` | private | 保留 | `trae` 有安全/断点/测试增强，先留 |
| `xinglin` | public | 活跃项目，保留 | 多条实现/部署分支，需项目内再合并整理 |
| `2025CodeRepository` | private | 临时留档 | `trae` 只有文档类改动，可删 |
| `TFTAssistant` | private | 保留 | 只有 `main` |
| `xinglin-core` | private | 保留 | 只有 `master` |
| `xinlingMain` | private | 归档保留 | 只有 `main` |

## 逐仓库分析

### 1. `MegaRepo`

定位：历史大杂烩仓，现在转为复活文档和索引仓。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 原索引/归档说明 | 不删 | 保留为默认分支 |
| `archive/revival-docs` | 本轮清理、复活、删除记录 | 不删 | 保留；这是恢复入口 |

仓库处理：保留，不再塞新代码。旧 `archive/*`、`feature/*`、`trae/*` 已删除远端，删除前 bundle 在 `_github-delete-backups`。

### 2. `PITS-Personal-Itinerary-Tracking-System`

定位：个人行程路径系统，MAUI/MVP 方向。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 旧基线 | 不删 | 保留默认，或后续由整合分支替换 |
| `codex/merge-pits-branches` | 已吸收两个 `trae` 的整合开发入口 | 不删 | 后续开发从这里切；最终合入 `main` |
| `trae/solo-agent-B7FdSg` | MVP 深度补齐、导入、轨迹、统计、测试 | 可删但先等 | 已被 `codex/merge-pits-branches` 吸收，确认后删 |
| `trae/solo-agent-SrlJMZ` | MVP/SVP art 与部分 UI 调整 | 可删但先等 | 已被 `codex/merge-pits-branches` 吸收，确认后删 |

仓库处理：保留。先把 `codex/merge-pits-branches` 作为真实主线跑构建，再决定是否合入 `main`。

### 3. `SpaceSniffMax` 私有

定位：Tauri/Rust/React 桌面磁盘分析器。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 很薄的默认分支 | 不直接删 | 先不要用它判断仓库价值 |
| `master` | 完整实现，含 Rust 后端、React 前端、扫描/缓存/AI 分类 | 不删 | 建议改成默认分支，或合并到 `main` |

仓库处理：保留。优先做分支纠偏，不做删除。

### 4. `Jvedio`

定位：本地视频管理软件。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `master` | 当前默认基线 | 不删 | 保留 |
| `dev-5.0` | v5/refactor 候选，metadata、scan、UI、tests 大改 | 不删 | 后续作为 v5 主线候选 |
| `kvdeio` | 图片收藏、NAS、ZSpace 实验 | 不删 | 作为实验参考，不要无脑并入 `dev-5.0` |

仓库处理：保留。后续按功能挑合，先清理 `.bak`、二进制和临时脚本。

### 5. `SSSSR`

定位：你后续还会提交的 .NET/WPF/Core 项目。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 唯一主线 | 不删 | 保留并继续开发 |

仓库处理：保留。

### 6. `yf.pt`

定位：信息流汇总平台。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 远端主线 | 不删 | 本地已领先 2 个提交，需推送 |
| `trae/agent-lLA2Bm` | 云资源、告警、成本等真实实现 | 可删但先等 | 本地已合并；推送 `main` 后可删远端分支 |

仓库处理：保留。下一步是推送本地 `main`，再删 `trae`。

### 7. `Feishu.Context`

定位：电商平台和扣子工作流集成的 WPF 项目。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 远端主线 | 不删 | 本地已领先 6 个提交，需推送 |
| `trae/solo-agent-vK7umW` | 数据库服务、配置、UI/业务功能增强 | 可删但先等 | 本地已合并；推送 `main` 后可删远端分支 |

仓库处理：保留。下一步是推送本地 `main`，再删 `trae`。

### 8. `DataForge.Core`

定位：.NET 数据处理/导入导出库。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 当前权威主线，已吸收 `trae` | 不删 | 保留 |
| `trae/solo-agent-xhpgEs` | 性能优化/管道改动的中间分支 | 可删 | 已是 `main` 的祖先，删除远端分支即可 |

仓库处理：保留。后续可做仓内瘦身，清理 `.sisyphus`、`.codebuddy` 等过程文件。

### 9. `ReportPlatform`

定位：报告单平台，活跃项目。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 当前默认主线，动态模板选择等实现 | 不删 | 保留 |
| `0520_master` | 报表编辑器核心重构、多适配器、PDF 导出优化 | 不删 | 活跃历史/候选主线，先保留 |
| `trae/solo-agent-D3YJ42` | code-wiki v2、文档/版本覆盖，含大量整理 | 暂不删 | 活跃项目内再判断 |

仓库处理：保留。不要在总清理阶段删分支，等项目内路线明确。

### 10. `MediaManager`

定位：音视频/媒体浏览管理。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 当前默认主线 | 不删 | 保留 |
| `trae/solo-agent-rLpN6W` | API、后台扫描、缓存、性能组件、Blazor 页面 | 不删 | 有真实实现，后续合并或作为候选主线 |

仓库处理：保留。后续跑构建和功能验证后决定是否合入 `main`。

### 11. `LocalDts` 私有

定位：数据迁移工具，独立私有仓是权威来源。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 当前主线 | 不删 | 保留 |
| `trae/solo-agent-v64RLs` | checkpoint/security/tests 增强 | 不删 | 有实现，后续合并前做构建/测试 |

仓库处理：保留。`MegaRepo/archive/localdts` 已被它覆盖。

### 12. `xinglin`

定位：病理检验报告系统，活跃项目。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 当前默认，偏精简 | 不删 | 保留 |
| `master` | 老主线/模板编辑器历史，含生成物 | 不删 | 先保留，后续清理生成物 |
| `feature/optimized-ui` | 优化 UI / 报告模板编辑器 | 不删 | 有实现，需项目内评估 |
| `web-enhanced` | Web 增强方向 | 不删 | 有实现，需项目内评估 |
| `gh-pages` | GitHub Pages 部署产物 | 可删但先等 | 如果不再用 Pages，可删 |
| `gh-pages-deploy` | Pages 部署分支 | 可删但先等 | 如果不用 Pages，可删 |
| `github-deployment` | 部署实验/发布分支 | 暂不删 | 与 Pages/部署有关，先确认 |
| `trae/solo-agent-4r3beL` | AI 整理/实现候选 | 暂不删 | 有大量实现差异，先项目内合并判断 |
| `trae/solo-agent-IsgOw4` | 缺陷与完善分类/实现候选 | 暂不删 | 有实现差异，先项目内合并判断 |
| `trae/solo-agent-Tw6Rsk` | GitHub Pages 构建和测试 | 暂不删 | 有实现/部署差异，先项目内判断 |
| `chore/cleanup-scripts` | cleanup 分支 | 可删 | 当前与 `main` 无差异 |
| `housekeeping/cleanup-scripts-automation` | housekeeping 分支 | 可删 | 当前与 `main` 无差异 |

仓库处理：保留。`xinglin` 分支多但不是垃圾仓，建议单独开一次项目内分支整合。

### 13. `2025CodeRepository` 私有

定位：2025 留档仓，当前已精简到 `FileExplorer`、`DingTalkLib`、`xfyun`。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 已精简留档主线 | 不删 | 保留，后期再拆 |
| `trae/solo-agent-zK4A1G` | 2 个文档类改动 | 可删 | 没有代码实现，删除远端分支即可 |

仓库处理：临时保留。长期应拆出有用内容后删除或归档。

### 14. `TFTAssistant` 私有

定位：游戏助手项目。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 唯一主线 | 不删 | 保留 |

仓库处理：保留。后续按兴趣和使用情况决定继续开发。

### 15. `xinglin-core` 私有

定位：杏林核心相关仓。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `master` | 唯一主线 | 不删 | 保留 |

仓库处理：保留。后续可考虑与 `xinglin` 主仓关系整理。

### 16. `xinlingMain` 私有

定位：杏林旧资料/附件仓。

| 分支 | 作用 | 可删除性 | 处理 |
| --- | --- | --- | --- |
| `main` | 唯一主线 | 暂不删 | 先归档保留 |

仓库处理：先保留，不继续开发。以后如果附件/旧资料已迁移，可改为本地归档或 mirror 后删远端。

## 已删除远端仓库的处理

这些仓库已从 GitHub 删除，但本地仍可恢复：

| 仓库 | 当前保留 | 处理 |
| --- | --- | --- |
| `SvgManager` | mirror | 已留档，不再占 GitHub 远端 |
| `avalonia-browser` | mirror | 已留档，不再占 GitHub 远端 |
| `autoCrafter` | mirror | 已留档，不再占 GitHub 远端 |
| `VibeSkills` | mirror | 已留档，不再占 GitHub 远端 |
| `vibe-coding-platform` | worktree + mirror | 模板参考，必要时本地复活 |
| `chatbot` | worktree + mirror | 模板参考，必要时本地复活 |
| `autoTicket` | worktree + mirror | 有实现分支，但风险高，只作本地复活参考 |

## 下一批可清理分支

可以优先删除的远端分支：

- `DataForge.Core/trae/solo-agent-xhpgEs`：已被 `main` 吸收。
- `2025CodeRepository/trae/solo-agent-zK4A1G`：只有文档类改动。
- `xinglin/chore/cleanup-scripts`：与 `main` 无差异。
- `xinglin/housekeeping/cleanup-scripts-automation`：与 `main` 无差异。

推送主线后再删除：

- `yf.pt/trae/agent-lLA2Bm`：本地已合并到 `main`，需先推送 `main`。
- `Feishu.Context/trae/solo-agent-vK7umW`：本地已合并到 `main`，需先推送 `main`。
- `PITS/trae/solo-agent-B7FdSg`、`PITS/trae/solo-agent-SrlJMZ`：已被 `codex/merge-pits-branches` 吸收，需先确认该整合分支就是后续入口。

先不删：

- `ReportPlatform/*`
- `MediaManager/trae/solo-agent-rLpN6W`
- `LocalDts/trae/solo-agent-v64RLs`
- `Jvedio/dev-5.0`
- `Jvedio/kvdeio`
- `SpaceSniffMax/master`
- `xinglin` 的实现/部署分支
