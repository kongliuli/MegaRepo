# GitHub 可见范围与清理策略

日期：2026-07-02
账号：`kongliuli`

## 当前 GitHub 连接可见范围

通过 Codex GitHub 连接确认登录账号为 `kongliuli`。

历史上 Codex GitHub MCP 曾直接可见 public owner 仓库；当前复核时 connector 返回空仓库、空 installed accounts、空 installations。因此后续仓库枚举以本机 `gh` CLI 为准。

2026-07-02 复核：`gh` 当前可见 23 个仓库，其中 14 个 public、9 个 private。逐仓库远端分支快照见 `GITHUB_REPOSITORY_BRANCH_SNAPSHOT.zh-CN.md`。

此前通过 connector 看到的 public 仓库：

| 仓库 | 可见性 | 状态 |
| --- | --- | --- |
| `autoCrafter` | public | 本地目录已删除，删除前已记录价值点 |
| `avalonia-browser` | public | 本地目录已删除，删除前已记录价值点 |
| `DataForge.Core` | public | 保留 |
| `Feishu.Context` | public | 保留，本地 `main` 已合并功能分支 |
| `Jvedio` | public | 保留观察，重点分支是 `kvdeio` 和 `dev-5.0` |
| `MediaManager` | public | 活跃项目，保留 |
| `MegaRepo` | public | 分支归档仓，已单独逐分支分析 |
| `PITS-Personal-Itinerary-Tracking-System` | public | 保留 |
| `ReportPlatform` | public | 活跃项目，保留 |
| `SSSSR` | public | 保留，后续继续提交 |
| `SvgManager` | public | 本地目录已删除，已形成留档说明 |
| `VibeSkills` | public | 本地目录已删除，已形成复活说明 |
| `xinglin` | public | 活跃项目，保留 |
| `yf.pt` | public | 保留，本地 `main` 已合并真实代码分支 |

GitHub MCP 当前未暴露私有仓库；GitHub CLI 登录后可以看到私有仓。

历史检查：

- owner 仓库：有 14 个，均为 public。
- collaborator 仓库：空。
- organization member 仓库：空。
- 组织列表：空。
- 后续分页：空。

结论：Codex GitHub MCP 不是本轮私有仓盘点的可靠来源；本机 GitHub CLI 已登录并能读取私有仓。

进一步复查结果：

- 早期 `owner,collaborator,organization_member` 组合查询只返回 14 个 public owner 仓库。
- 当前复核时 connector 返回空仓库、空 `installed_accounts`、空 `installations`。
- 本机已安装 `gh` CLI，且账号 `kongliuli` 的 `repo` scope 可读取私有仓。
- GitHub CLI 当前能看到 23 个仓库，其中 9 个私有仓；私有仓均已克隆或本地已存在。
- GitHub connector/MCP 仍未暴露 installed accounts / installations，所以 connector 查询结果不能代表全部仓库。

所以这里是“Codex GitHub MCP/connector 没有可用仓库授权范围”，不是没有私有仓。

当前可用路径：

1. 继续使用已安装的 `gh` CLI 查询和克隆私有仓。
2. 如果要让 MCP 也能直接看到私有仓，需要在 Codex GitHub 连接里授权私有仓给这个 App。

## 清理原则

你的目标不是简单删除，而是：

1. 完全不需要的仓库可以清理。
2. 有价值的点先形成文档。
3. 后续如果需要，根据文档复活。
4. 后续会在本地按不同分支继续开发；默认分支不完整时，必须先看其他远端分支，不能按默认分支误删。

执行顺序：

1. 先盘点证据。
2. 再写中文留档说明。
3. 再删除或归档仓库。

删除前最少保留：

- 项目定位。
- 核心功能。
- 可复用代码路径。
- 不保留原因。
- 后续复活路径。

## 当前已形成的留档文档

| 文档 | 用途 |
| --- | --- |
| `REPO_AUDIT_REPORT.zh-CN.md` | 总体仓库盘点 |
| `GITHUB_REPOSITORY_BRANCH_SNAPSHOT.zh-CN.md` | 当前 GitHub 可见仓库和远端分支快照 |
| `DATAFORGE_CORE_BRANCH_NOTE.zh-CN.md` | `DataForge.Core` 分支吸收关系说明 |
| `PITS_BRANCH_NOTE.zh-CN.md` | `PITS` 合并分支和后续开发入口说明 |
| `MEGAREPO_BRANCH_AUDIT.zh-CN.md` | `MegaRepo` 每个分支分析 |
| `MEGAREPO_REVIVAL_NOTES.zh-CN.md` | `MegaRepo` 候选分支复活说明 |
| `SVGMANAGER_ARCHIVE_NOTE.zh-CN.md` | `SvgManager` 删除前留档说明 |
| `AVALONIA_BROWSER_ARCHIVE_NOTE.zh-CN.md` | `avalonia-browser` 删除前复活说明 |
| `AUTOCRAFTER_ARCHIVE_NOTE.zh-CN.md` | `autoCrafter` 删除前复活说明 |
| `VIBESKILLS_ARCHIVE_NOTE.zh-CN.md` | `VibeSkills` 删除前复活说明 |
| `DELETE_QUEUE.zh-CN.md` | 当前可删除候选队列 |
| `PRIVATE_REPO_AUDIT.zh-CN.md` | 私有仓库盘点 |
| `SPACESNIFFMAX_BRANCH_NOTE.zh-CN.md` | `SpaceSniffMax` 分支纠偏与保留说明 |
| `PRIVATE_BRANCH_AUDIT.zh-CN.md` | 私有仓库逐分支盘点 |
| `VIBE_CODING_PLATFORM_ARCHIVE_NOTE.zh-CN.md` | `vibe-coding-platform` 删除前复活说明 |
| `CHATBOT_ARCHIVE_NOTE.zh-CN.md` | `chatbot` 删除前复活说明 |
| `AUTOTICKET_ARCHIVE_NOTE.zh-CN.md` | `autoTicket` 删除前复活说明 |
| `LOCALDTS_RETENTION_NOTE.zh-CN.md` | `LocalDts` 保留说明 |
| `TFTASSISTANT_RETENTION_NOTE.zh-CN.md` | `TFTAssistant` 保留说明 |
| `XINGLIN_CORE_RETENTION_NOTE.zh-CN.md` | `xinglin-core` 保留说明 |
| `XINLINGMAIN_ARCHIVE_NOTE.zh-CN.md` | `xinlingMain` 归档说明 |
| `JVEDIO_BRANCH_NOTE.zh-CN.md` | `Jvedio` 的 `dev-5.0` / `kvdeio` 分支保留说明 |

## 已删除本地目录

| 仓库 | 建议 |
| --- | --- |
| `SvgManager` | 本地目录已删除，远端未删除 |
| `avalonia-browser` | 本地目录已删除，远端未删除 |
| `autoCrafter` | 本地目录已删除，远端未删除 |
| `VibeSkills` | 本地目录已删除，远端未删除 |

## 待明确确认删除

| 仓库 | 建议 |
| --- | --- |
| `vibe-coding-platform` | 复活说明已写，可在你明确确认后删除 |
| `chatbot` | 复活说明已写，可在你明确确认后删除 |
| `autoTicket` | 复活说明已写，可在你明确确认后删除 |

## 需要保留或继续推进

| 仓库 | 原因 |
| --- | --- |
| `SSSSR` | 你后续还有提交 |
| `yf.pt` | 已合并真实代码分支，构建通过 |
| `DataForge.Core` | 完整库项目；`trae` 分支已被 `main` 吸收 |
| `Feishu.Context` | 已合并功能分支 |
| `SpaceSniffMax` | `origin/master` 是完整 Tauri/Rust/React 磁盘分析器，保留 |
| `Jvedio` | 后续可能按分支本地开发；`origin/dev-5.0` 和 `origin/kvdeio` 都有有效内容 |
| `LocalDts` | 私有数据迁移工具，`trae` 分支也有有效增强 |
| `TFTAssistant` | 完整游戏助手项目，有测试，暂不删 |
| `xinglin-core` | 杏林核心相关，保留 |
| `xinlingMain` | 杏林旧资料/附件仓，先归档不删 |
| `PITS-Personal-Itinerary-Tracking-System` | 保留；`codex/merge-pits-branches` 是后续开发入口 |
| `ReportPlatform` | 活跃项目 |
| `xinglin` | 活跃项目 |
| `MediaManager` | 活跃项目 |

## 仍需关注

| 仓库/分支 | 原因 |
| --- | --- |
| `MegaRepo/archive/localdts` | 已被独立私有仓 `LocalDts` 覆盖，不再从 MegaRepo 复活 |
| `MegaRepo/feature/outpatient-inventory` | 已复查，可删，只留门诊库存模型/分层想法 |
| `MegaRepo/feature/programcell` | 已复查，可删，只留 GitLab/Jenkins 看板想法 |
| `MegaRepo/feature/yunwei-tool` | 已复查，可删，只留项目/环境模型想法 |
| `Jvedio/dev-5.0` | 已形成分支说明；作为 v5/refactor 主线候选保留 |
| `Jvedio/kvdeio` | 已形成分支说明；作为图片/NAS/ZSpace 实验分支保留 |
