# 复活文档索引

日期：2026-07-02
分支：`archive/revival-docs`

这个分支只存本轮 GitHub 仓库清理和复活文档。

用途：

- 删除或归档远端分支后，仍能看到每个分支为什么删、以后怎么复活。
- 删除 GitHub 仓库前，记录本地 mirror 备份位置和删除步骤。
- 避免 `MegaRepo` 继续承担大杂烩代码仓职责。

核心文档：

| 文档 | 用途 |
| --- | --- |
| `MEGAREPO_BRANCH_AUDIT.zh-CN.md` | `MegaRepo` 远端分支逐项分析 |
| `MEGAREPO_REVIVAL_NOTES.zh-CN.md` | `MegaRepo` 候选分支复活路径 |
| `MEGAREPO_BRANCH_TIPS_BEFORE_DELETE.zh-CN.md` | 删除远端分支前的 commit tip 记录 |
| `GITHUB_REPOSITORY_DELETE_PLAN.zh-CN.md` | GitHub 仓库删除和本地保留方案 |
| `DELETE_QUEUE.zh-CN.md` | 当前本地删除候选队列 |
| `GITHUB_REPOSITORY_BRANCH_SNAPSHOT.zh-CN.md` | GitHub 可见仓库和分支快照 |

本地额外保留：

- `D:\Code\githubDown\_github-delete-backups\MegaRepo-before-branch-cleanup-20260702.bundle`
- `D:\Code\githubDown\_github-delete-backups\<repo>.git`

后续如果要恢复 `MegaRepo` 被删除的远端分支，优先从 bundle 或本地 `MegaRepo` 仓库恢复。
