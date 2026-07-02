# 删除候选队列

日期：2026-07-02
目录：`D:\Code\githubDown`

## 目的

先把有价值的点写成复活文档，再删除本体。删除动作需要你明确点名确认。

补充原则：你后续会在本地针对不同分支继续开发，所以本轮删除只处理明显无用、模板化或复活价值低的仓库。像 `SpaceSniffMax`、`Jvedio` 这类“默认分支不代表真实价值”的仓库，先保留。

## 已删除本地目录

只删除了本地目录，没有删除 GitHub 远端仓库。

| 仓库 | 原路径 | 留档文档 | 删除结果 |
| --- | --- | --- | --- |
| `SvgManager` | `D:\Code\githubDown\SvgManager` | `SVGMANAGER_ARCHIVE_NOTE.zh-CN.md` | 已删除 |
| `avalonia-browser` | `D:\Code\githubDown\avalonia-browser` | `AVALONIA_BROWSER_ARCHIVE_NOTE.zh-CN.md` | 已删除 |
| `autoCrafter` | `D:\Code\githubDown\autoCrafter` | `AUTOCRAFTER_ARCHIVE_NOTE.zh-CN.md` | 已删除 |
| `VibeSkills` | `D:\Code\githubDown\VibeSkills` | `VIBESKILLS_ARCHIVE_NOTE.zh-CN.md` | 已删除 |

## 待明确确认删除

如果要继续删除，请直接点名这三个本地目录，例如：`删除 vibe-coding-platform、chatbot、autoTicket 本地目录`。

| 仓库 | 当前路径 | 状态 | 复活文档 | 删除前结论 |
| --- | --- | --- | --- | --- |
| `vibe-coding-platform` | `D:\Code\githubDown\vibe-coding-platform` | 目录存在 | `VIBE_CODING_PLATFORM_ARCHIVE_NOTE.zh-CN.md` | 可删 |
| `chatbot` | `D:\Code\githubDown\chatbot` | 目录存在 | `CHATBOT_ARCHIVE_NOTE.zh-CN.md` | 可删 |
| `autoTicket` | `D:\Code\githubDown\autoTicket` | 目录存在 | `AUTOTICKET_ARCHIVE_NOTE.zh-CN.md` | 可删 |

## 暂不删除

| 仓库 | 原因 |
| --- | --- |
| `SSSSR` | 保留，后续继续提交 |
| `yf.pt` | 已合并真实代码分支，构建通过 |
| `DataForge.Core` | 保留，`trae` 分支已被 `main` 吸收，见 `DATAFORGE_CORE_BRANCH_NOTE.zh-CN.md` |
| `Feishu.Context` | 保留，本地 `main` 已合并功能分支 |
| `SpaceSniffMax` | 保留，完整项目在 `origin/master`，不能按默认 `origin/main` 误删 |
| `LocalDts` | 保留，私有仓价值高，见 `LOCALDTS_RETENTION_NOTE.zh-CN.md` |
| `TFTAssistant` | 暂不删除，完整项目且有测试，见 `TFTASSISTANT_RETENTION_NOTE.zh-CN.md` |
| `xinglin-core` | 保留，杏林核心相关，见 `XINGLIN_CORE_RETENTION_NOTE.zh-CN.md` |
| `xinlingMain` | 先归档不删除，见 `XINLINGMAIN_ARCHIVE_NOTE.zh-CN.md` |
| `PITS-Personal-Itinerary-Tracking-System` | 保留，`codex/merge-pits-branches` 是后续开发入口，见 `PITS_BRANCH_NOTE.zh-CN.md` |
| `ReportPlatform` | 活跃项目 |
| `xinglin` | 活跃项目 |
| `MediaManager` | 活跃项目 |
| `Jvedio` | 保留，`dev-5.0` / `kvdeio` 分支有明显有效改动，见 `JVEDIO_BRANCH_NOTE.zh-CN.md` |
| `MegaRepo` | 保留，旧远端分支已清理；复活文档在 `archive/revival-docs` |
| `2025CodeRepository` | 临时留档，已精简 |

## 删除方式

只删除本地目录，不删除 GitHub 远端仓库。

当前待确认候选共 3 个：

```powershell
Remove-Item -LiteralPath "D:\Code\githubDown\vibe-coding-platform" -Recurse -Force
Remove-Item -LiteralPath "D:\Code\githubDown\chatbot" -Recurse -Force
Remove-Item -LiteralPath "D:\Code\githubDown\autoTicket" -Recurse -Force
```

删除前我会再次校验路径都在 `D:\Code\githubDown` 下。

## GitHub 可见范围复查

当前 GitHub 连接复查结果：

- GitHub connector/MCP 当前返回空仓库、空 installed accounts、空 installations。
- GitHub CLI 可看到 23 个仓库，其中 9 个私有仓；已形成远端分支快照。

因此本轮清理基于本地仓库，以及 GitHub CLI 读取到的私有仓。
