# GitHub 仓库与分支快照

日期：2026-07-02
账号：`kongliuli`

## 可见范围

当前以本机 GitHub CLI 为准：

- 可见仓库：23 个。
- public：14 个。
- private：9 个。
- Codex GitHub connector 当前仍返回空 installed accounts / installations / repositories。

## 分支快照

| 仓库 | 可见性 | 默认分支 | 远端分支 | 判断 |
| --- | --- | --- | --- | --- |
| `PITS-Personal-Itinerary-Tracking-System` | public | `main` | `codex/merge-pits-branches`, `main`, `trae/solo-agent-B7FdSg`, `trae/solo-agent-SrlJMZ` | 保留；`codex/merge-pits-branches` 已吸收两个 `trae` 分支，作为后续开发入口 |
| `MegaRepo` | public | `main` | `archive/localdts`, `archive/mindmap`, `feature/autoticket`, `feature/autowebclient`, `feature/blazor-trae`, `feature/dbconnection-wpf`, `feature/dingtalklib`, `feature/diting-utils`, `feature/fileexplorer`, `feature/outpatient-inventory`, `feature/programcell`, `feature/tools-components`, `feature/universal-invoice`, `feature/yunwei-tool`, `main`, `trae/solo-agent-edXDXQ` | 分支归档仓，已单独分析 |
| `SpaceSniffMax` | private | `main` | `main`, `master` | 保留；完整内容在 `master` |
| `Jvedio` | public | `master` | `dev-5.0`, `kvdeio`, `master` | 保留；两个非默认分支都有价值 |
| `SSSSR` | public | `main` | `main` | 保留 |
| `yf.pt` | public | `main` | `main`, `trae/agent-lLA2Bm` | 已合并功能分支到本地 `main` |
| `Feishu.Context` | public | `main` | `main`, `trae/solo-agent-vK7umW` | 已合并功能分支到本地 `main` |
| `DataForge.Core` | public | `main` | `main`, `trae/solo-agent-xhpgEs` | 保留；`trae` 分支已被 `main` 吸收，后续从 `main` 开发 |
| `ReportPlatform` | public | `main` | `0520_master`, `main`, `trae/solo-agent-D3YJ42` | 活跃项目，保留 |
| `MediaManager` | public | `main` | `main`, `trae/solo-agent-rLpN6W` | 活跃项目，保留 |
| `LocalDts` | private | `main` | `main`, `trae/solo-agent-v64RLs` | 保留 |
| `xinglin` | public | `main` | `chore/cleanup-scripts`, `feature/optimized-ui`, `gh-pages`, `gh-pages-deploy`, `github-deployment`, `housekeeping/cleanup-scripts-automation`, `main`, `master`, `trae/solo-agent-4r3beL`, `trae/solo-agent-IsgOw4`, `trae/solo-agent-Tw6Rsk`, `web-enhanced` | 活跃项目，保留 |
| `vibe-coding-platform` | private | `main` | `main` | 待确认删除本地目录 |
| `chatbot` | private | `main` | `main` | 待确认删除本地目录 |
| `autoTicket` | private | `main` | `main`, `trae/solo-agent-4wA9Gt`, `trae/solo-agent-9WDg6n`, `trae/solo-agent-MHmYQC`, `trae/solo-agent-vYyGSS`, `trae/solo-agent-yczxWm` | 待确认删除本地目录；保留复活文档即可 |
| `SvgManager` | public | `main` | `main` | 本地已删，远端未删 |
| `autoCrafter` | public | `main` | `main`, `trae/solo-agent-ZNp5vq` | 本地已删，远端未删 |
| `2025CodeRepository` | private | `main` | `main`, `trae/solo-agent-zK4A1G` | 本地已精简，后续再拆 |
| `TFTAssistant` | private | `main` | `main` | 保留 |
| `xinglin-core` | private | `master` | `master` | 保留 |
| `VibeSkills` | public | `main` | `main` | 本地已删，远端未删 |
| `avalonia-browser` | public | `main` | `main` | 本地已删，远端未删 |
| `xinlingMain` | private | `main` | `main` | 先归档不删 |

## 清理含义

当前没有发现新的漏克隆私有仓。真正还在本地、且已进入待确认删除队列的仓库仍是：

- `vibe-coding-platform`
- `chatbot`
- `autoTicket`

其他仓库要么活跃，要么存在非默认分支价值，要么已经删掉本地目录。
