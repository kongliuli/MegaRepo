# 剩余分支实现性复查

日期：2026-07-02
目录：`D:\Code\githubDown`

## 结论

有。剩余需要整理的分支里，确实有不少分支包含真实实现和提交，不是纯文档。

最应该继续保留/整理的实现分支：

| 仓库 | 分支 | 判断 |
| --- | --- | --- |
| `SpaceSniffMax` | `origin/master` | 完整 Tauri/Rust/React 实现，默认 `main` 很薄，必须保留 |
| `Jvedio` | `origin/dev-5.0` | 大规模 v5/refactor 候选，有 metadata、scan、UI、test 改动 |
| `Jvedio` | `origin/kvdeio` | 图片收藏、NAS、ZSpace 实验分支，有实现 |
| `LocalDts` | `origin/trae/solo-agent-v64RLs` | checkpoint/security/tests 增强，有实现 |
| `PITS-Personal-Itinerary-Tracking-System` | `origin/codex/merge-pits-branches` | 已吸收两个 `trae` 分支，是后续开发入口 |
| `MediaManager` | `origin/trae/solo-agent-rLpN6W` | 新增 API/后台扫描/Blazor 页面，有实现 |
| `ReportPlatform` | `origin/0520_master`、`origin/trae/solo-agent-D3YJ42` | 活跃项目分支，有大量实现和历史内容 |
| `xinglin` | `master`、`web-enhanced`、`feature/optimized-ui`、`trae/*` 等 | 活跃项目分支，有大量 WPF/Web/部署相关实现 |

已经被主线或整理分支吸收，不需要再单独当入口的分支：

| 仓库 | 分支 | 判断 |
| --- | --- | --- |
| `DataForge.Core` | `origin/trae/solo-agent-xhpgEs` | 已被 `origin/main` 吸收，后续从 `main` 开发 |
| `Feishu.Context` | `origin/trae/solo-agent-vK7umW` | 已合并到本地 `main`，待推送 |
| `yf.pt` | `origin/trae/agent-lLA2Bm` | 已合并到本地 `main`，待推送 |
| `PITS-Personal-Itinerary-Tracking-System` | `origin/trae/solo-agent-B7FdSg`、`origin/trae/solo-agent-SrlJMZ` | 已被 `codex/merge-pits-branches` 吸收 |

已经删除 GitHub 远端、只作本地复活参考的分支：

| 仓库 | 分支 | 判断 |
| --- | --- | --- |
| `autoTicket` | `origin/trae/solo-agent-4wA9Gt` 等 | 有实现，但远端仓库已删除；本地目录和 mirror 保留 |
| `vibe-coding-platform` | `origin/main` | 模板仓，远端已删，本地保留 |
| `chatbot` | `origin/main` | 模板仓，远端已删，本地保留 |

纯文档/过程或无差异分支：

| 仓库 | 分支 | 判断 |
| --- | --- | --- |
| `MegaRepo` | `origin/archive/revival-docs` | 复活文档分支，不是代码实现 |
| `2025CodeRepository` | `origin/trae/solo-agent-zK4A1G` | 只有 2 个文档类改动，当前以已精简 `main` 为准 |
| `xinglin` | `chore/cleanup-scripts`、`housekeeping/cleanup-scripts-automation` | 当前与 `main` 无差异或只作 housekeeping |

## 下一步建议

1. 活跃项目 `ReportPlatform`、`xinglin`、`MediaManager` 先不删，后续按项目内部路线整理。
2. `SpaceSniffMax`、`Jvedio`、`LocalDts`、`PITS` 都有非默认分支价值，不能按默认分支误删。
3. 已被吸收的分支可以后续清理远端，但要先确认对应主线已推送。
4. 已删远端仓库只从本地 mirror 或工作区复活，不再依赖 GitHub。
