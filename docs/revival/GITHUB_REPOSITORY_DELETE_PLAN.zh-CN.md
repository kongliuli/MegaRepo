# GitHub 仓库删除计划

日期：2026-07-02
目录：`D:\Code\githubDown`

## 原则

删除 GitHub 远端仓库前，必须本地保留。

本地保留满足其一即可：

- `D:\Code\githubDown\<repo>` 存在普通 working copy。
- `D:\Code\githubDown\_github-delete-backups\<repo>.git` 存在 mirror 备份。

远端删除不是本轮自动动作。必须你再次点名仓库后再执行。

## 当前远端仓库删除候选

| 仓库 | 远端状态 | 本地保留状态 | 说明 |
| --- | --- | --- | --- |
| `SvgManager` | public | 已有 mirror：`_github-delete-backups\SvgManager.git` | 本地目录已删，已有复活文档 |
| `avalonia-browser` | public | 已有 mirror：`_github-delete-backups\avalonia-browser.git` | 本地目录已删，已有复活文档 |
| `autoCrafter` | public | 已有 mirror：`_github-delete-backups\autoCrafter.git` | 本地目录已删，已有复活文档 |
| `VibeSkills` | public | 已有 mirror：`_github-delete-backups\VibeSkills.git` | 本地目录已删，已有复活文档 |
| `vibe-coding-platform` | private | 本地目录存在，另有 mirror：`_github-delete-backups\vibe-coding-platform.git` | 模板仓，已有复活文档 |
| `chatbot` | private | 本地目录存在，另有 mirror：`_github-delete-backups\chatbot.git` | 模板仓，已有复活文档 |
| `autoTicket` | private | 本地目录存在，另有 mirror：`_github-delete-backups\autoTicket.git` | 风险和维护成本高，已有复活文档 |

不建议远端删除：

- `SpaceSniffMax`：完整内容在 `master`。
- `Jvedio`：`dev-5.0` / `kvdeio` 都有价值。
- `DataForge.Core`：主线已吸收 `trae` 分支。
- `PITS-Personal-Itinerary-Tracking-System`：`codex/merge-pits-branches` 是后续开发入口。
- `LocalDts`、`TFTAssistant`、`xinglin-core`、`xinlingMain`、`SSSSR`、`yf.pt`、`Feishu.Context`、`ReportPlatform`、`xinglin`、`MediaManager`。

## 执行方式

当前 `gh auth status` 显示 token scope 包含：

- `gist`
- `read:org`
- `repo`
- `workflow`

删除仓库通常还需要 `delete_repo` scope。执行前先刷新授权：

```powershell
& 'C:\Program Files\GitHub CLI\gh.exe' auth refresh -h github.com -s delete_repo
```

然后使用带保护检查的脚本：

```powershell
.\delete-github-repos-after-backup.ps1 -Repo SvgManager,avalonia-browser -ConfirmRemoteDeletion
```

脚本会逐个检查本地保留是否存在。没有本地保留时会停止，不删除远端。

## MegaRepo 分支删除

`MegaRepo` 远端分支删除前，先保留：

- `D:\Code\githubDown\_github-delete-backups\MegaRepo-before-branch-cleanup-20260702.bundle`
- `MegaRepo` 新分支 `archive/revival-docs`

`archive/revival-docs` 用于保存本轮仓库盘点和复活文档。
