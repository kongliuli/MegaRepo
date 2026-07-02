# GitHub 仓库与分支快照

日期：2026-07-02
账号：`kongliuli`

## 可见范围

当前以本机 GitHub CLI 为准：

- 可见仓库：16 个。
- public：10 个。
- private：6 个。
- Codex GitHub connector 当前仍返回空 installed accounts / installations / repositories。

## 分支快照

| 仓库 | 可见性 | 默认分支 | 远端分支 | 判断 |
| --- | --- | --- | --- | --- |
| `PITS-Personal-Itinerary-Tracking-System` | public | `main` | `codex/merge-pits-branches`, `main`, `trae/solo-agent-B7FdSg`, `trae/solo-agent-SrlJMZ` | 保留；`codex/merge-pits-branches` 已吸收两个 `trae` 分支，作为后续开发入口 |
| `MegaRepo` | public | `main` | `archive/revival-docs`, `main` | 已清理旧远端分支；复活文档保存在 `archive/revival-docs` |
| `SpaceSniffMax` | private | `master` | `main`, `master` | 保留；完整内容在 `master`，默认分支已纠偏 |
| `Jvedio` | public | `master` | `dev-5.0`, `kvdeio`, `master` | 保留；两个非默认分支都有价值 |
| `SSSSR` | public | `main` | `main` | 保留 |
| `yf.pt` | public | `main` | `main` | 已合并并删除 `trae` 远端分支 |
| `Feishu.Context` | public | `main` | `main` | 已合并并删除 `trae` 远端分支 |
| `DataForge.Core` | public | `main` | `main` | 保留；`trae` 分支已被 `main` 吸收并删除 |
| `ReportPlatform` | public | `main` | `0520_master`, `main`, `trae/solo-agent-D3YJ42` | 活跃项目，保留 |
| `MediaManager` | public | `main` | `main`, `trae/solo-agent-rLpN6W` | 活跃项目，保留 |
| `LocalDts` | private | `main` | `main`, `trae/solo-agent-v64RLs` | 保留 |
| `xinglin` | public | `main` | `feature/optimized-ui`, `gh-pages`, `gh-pages-deploy`, `github-deployment`, `main`, `master`, `trae/solo-agent-4r3beL`, `trae/solo-agent-IsgOw4`, `trae/solo-agent-Tw6Rsk`, `web-enhanced` | 活跃项目，保留；两个无差异 cleanup 分支已删 |
| `2025CodeRepository` | private | `main` | `main` | 本地已精简并推送，文档类 `trae` 分支已删 |
| `TFTAssistant` | private | `main` | `main` | 保留 |
| `xinglin-core` | private | `master` | `master` | 保留 |
| `xinlingMain` | private | `main` | `main` | 先归档不删 |

## 清理含义

当前没有发现新的漏克隆私有仓。

## 已删除 GitHub 远端仓库

这些仓库已从 GitHub 删除，但本地保留仍在：

| 仓库 | 原可见性 | 本地保留 |
| --- | --- | --- |
| `SvgManager` | public | mirror：`_github-delete-backups\SvgManager.git` |
| `avalonia-browser` | public | mirror：`_github-delete-backups\avalonia-browser.git` |
| `autoCrafter` | public | mirror：`_github-delete-backups\autoCrafter.git` |
| `VibeSkills` | public | mirror：`_github-delete-backups\VibeSkills.git` |
| `vibe-coding-platform` | private | worktree + mirror：`_github-delete-backups\vibe-coding-platform.git` |
| `chatbot` | private | worktree + mirror：`_github-delete-backups\chatbot.git` |
| `autoTicket` | private | worktree + mirror：`_github-delete-backups\autoTicket.git` |

其他仓库要么活跃，要么存在非默认分支价值，要么已经删掉 GitHub 远端并保留本地备份。
