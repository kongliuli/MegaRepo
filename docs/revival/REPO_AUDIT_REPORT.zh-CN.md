# GitHub 仓库盘点报告

日期：2026-07-02
目录：`D:\Code\githubDown`

## GitHub 连接补充

已通过 Codex GitHub 连接确认账号为 `kongliuli`。

Codex GitHub MCP/connector 当前没有返回可用仓库或安装账号；本机 GitHub CLI 已登录，当前可见 23 个仓库，其中 9 个私有仓。详细记录见 `GITHUB_ACCESS_AND_CLEANUP_PLAN.zh-CN.md`、`PRIVATE_REPO_AUDIT.zh-CN.md` 和 `GITHUB_REPOSITORY_BRANCH_SNAPSHOT.zh-CN.md`。

## 当前状态

当前本地保留 19 个 Git 仓库；所有已检查仓库工作区都是干净的。

补充判断：你后续会在本地针对不同分支继续开发，所以本轮清理目标是减少无用仓库对判断的干扰，而不是只按默认分支完整度删仓库。默认分支很薄但其他分支完整的仓库，先保留。

本地领先远端的仓库：

| 仓库 | 状态 |
| --- | --- |
| `2025CodeRepository` | `main` 领先 `origin/main` 1 个清理提交：`685b5f2 chore: trim archive to retained projects` |
| `Feishu.Context` | `main` 在合并 `origin/trae/solo-agent-vK7umW` 后领先 `origin/main` 6 个提交 |
| `yf.pt` | `main` 在合并真实代码分支后领先 `origin/main` 2 个提交 |

未执行 push。

## 活跃仓库，不参与删减讨论

这些仓库保留，后续整理好继续推进。

| 仓库 | 结论 | 依据 |
| --- | --- | --- |
| `ReportPlatform` | 保留，活跃项目 | 医疗报告平台；`main`、`0520_master`、`trae/solo-agent-D3YJ42` 都有有效内容 |
| `xinglin` | 保留，活跃项目 | 病理检验报告系统；`main` 较精简，`master/web/trae` 系列分支保留了更多历史和产品内容 |
| `MediaManager` | 保留，活跃项目 | 结构完整，有测试报告；`trae` 分支新增 API 项目 |

## 已处理仓库

| 仓库 | 结论 | 备注 |
| --- | --- | --- |
| `2025CodeRepository` | 临时留档 | 已清理为只保留 `FileExplorer`、`DingTalkLib`、`xfyun`，并标注后期再拆 |
| `Feishu.Context` | 保留 | 已把 `origin/trae/solo-agent-vK7umW` 合并到本地 `main`，之前构建通过 |
| `DataForge.Core` | 保留 | 库项目结构完整，有测试；`trae` 分支已被 `main` 吸收，见 `DATAFORGE_CORE_BRANCH_NOTE.zh-CN.md` |
| `PITS-Personal-Itinerary-Tracking-System` | 保留 | 当前在 `codex/merge-pits-branches` 分支；该分支已吸收两个 `trae` 分支，见 `PITS_BRANCH_NOTE.zh-CN.md` |
| `yf.pt` | 保留 | 已合并 `origin/trae/agent-lLA2Bm` 到 `main`，构建通过 |
| `SvgManager` | 已删除本地目录 | 删除前已形成 `SVGMANAGER_ARCHIVE_NOTE.zh-CN.md` |
| `avalonia-browser` | 已删除本地目录 | 删除前已形成 `AVALONIA_BROWSER_ARCHIVE_NOTE.zh-CN.md` |
| `autoCrafter` | 已删除本地目录 | 删除前已形成 `AUTOCRAFTER_ARCHIVE_NOTE.zh-CN.md` |
| `VibeSkills` | 已删除本地目录 | 删除前已形成 `VIBESKILLS_ARCHIVE_NOTE.zh-CN.md` |

## 私有仓库结论

当前通过 GitHub CLI 看到 9 个私有仓，并已逐分支检查；同时已对当前可见的 23 个仓库形成远端分支快照。细节见 `PRIVATE_REPO_AUDIT.zh-CN.md`、`PRIVATE_BRANCH_AUDIT.zh-CN.md`、`GITHUB_REPOSITORY_BRANCH_SNAPSHOT.zh-CN.md`。

| 仓库 | 结论 | 依据 |
| --- | --- | --- |
| `SpaceSniffMax` | 保留 | 默认 `origin/main` 很薄，但完整项目在 `origin/master`；见 `SPACESNIFFMAX_BRANCH_NOTE.zh-CN.md` |
| `LocalDts` | 保留 | 独立数据迁移工具，`trae` 分支有 checkpoint/security/tests 增强；见 `LOCALDTS_RETENTION_NOTE.zh-CN.md` |
| `vibe-coding-platform` | 可删 | Vercel AI coding 模板，已写复活说明：`VIBE_CODING_PLATFORM_ARCHIVE_NOTE.zh-CN.md` |
| `chatbot` | 可删 | Vercel AI SDK chatbot 模板，已写复活说明：`CHATBOT_ARCHIVE_NOTE.zh-CN.md` |
| `autoTicket` | 可删 | 自动票务方向，风险和维护成本高，已写复活说明：`AUTOTICKET_ARCHIVE_NOTE.zh-CN.md` |
| `TFTAssistant` | 暂不删 | 完整、有测试；见 `TFTASSISTANT_RETENTION_NOTE.zh-CN.md` |
| `xinglin-core` | 保留 | 杏林核心相关；见 `XINGLIN_CORE_RETENTION_NOTE.zh-CN.md` |
| `xinlingMain` | 先归档，不删 | 杏林旧资料/附件仓；见 `XINLINGMAIN_ARCHIVE_NOTE.zh-CN.md` |

## Jvedio

确认存在的重要分支：

| 分支 | 最新提交 | 证据 |
| --- | --- | --- |
| `origin/master` | `0d29c1d 新增图片模式 (todo)` | 当前检出分支 |
| `origin/dev-5.0` | `61a580e Remove accidental nested Jvedio submodule stub` | 相对 `master` 变更 261 个文件，主要集中在 `Jvedio-WPF/Jvedio` 和测试 |
| `origin/kvdeio` | `a3c8e43 Add user picture collections and ZSpace NAS research (Phase D)` | 相对 `master` 变更 292 个文件，主要集中在 `Jvedio-WPF/Jvedio` 和测试 |

没有字面叫 `v-5` 的远端分支。若你说的 v5 是版本线，当前应按 `origin/dev-5.0` 处理。

结论：`Jvedio` 保留。`origin/dev-5.0` 作为 v5/refactor 主线候选；`origin/kvdeio` 作为图片收藏、NAS、ZSpace 实验分支保留。两者都不适合无脑合并，但都足以影响后续本地开发判断。详细见 `JVEDIO_BRANCH_NOTE.zh-CN.md`。

## 需要决策的仓库

| 仓库 | 结论 | 最小动作 |
| --- | --- | --- |
| `MegaRepo` | 暂时保留为分支归档仓 | 不要从整分支复活代码；`LocalDts` 用独立私有仓，其余只参考文档想法 |

## 倾向保留

| 仓库 | 结论 | 原因 |
| --- | --- | --- |
| `SSSSR` | 保留 | 你后续还有提交；真实 .NET 10 WPF/Core 项目，有测试 |

## 倾向归档或删除

| 仓库 | 结论 | 原因 |
| --- | --- | --- |
| `vibe-coding-platform` | 待你明确确认后删除 | 已形成复活说明：`VIBE_CODING_PLATFORM_ARCHIVE_NOTE.zh-CN.md` |
| `chatbot` | 待你明确确认后删除 | 已形成复活说明：`CHATBOT_ARCHIVE_NOTE.zh-CN.md` |
| `autoTicket` | 待你明确确认后删除 | 已形成复活说明：`AUTOTICKET_ARCHIVE_NOTE.zh-CN.md` |

## MegaRepo 分支判断

| 分支 | 结论 | 原因 |
| --- | --- | --- |
| `archive/localdts` | 不再从 MegaRepo 复活 | 已被独立私有仓 `LocalDts` 覆盖 |
| `archive/mindmap` | 只归档 | 小型 Web/JS 思维导图/大纲工具，无项目文件 |
| `feature/autoticket` | 删除或归档 | 自动购票工具，旧 WinForms/.NET 5 风格，风险高、复用价值低 |
| `feature/autowebclient` | 删除或归档 | 老 CEFSharp 自动化集合，包含购票/支付抓取类工具 |
| `feature/blazor-trae` | 归档或删除 | Blazor demo，包含地图、天气、todo，并且分支里有生成的 `obj` 文件 |
| `feature/dbconnection-wpf` | 删除或归档 | 之前在 `2025CodeRepository` 已决定不保留 |
| `feature/dingtalklib` | 重复 | 已在 `2025CodeRepository` 留档，无需保留两份 |
| `feature/diting-utils` | 删除或归档 | 通用 utils，独特价值低 |
| `feature/fileexplorer` | 重复 | 已在 `2025CodeRepository` 留档，无需保留两份 |
| `feature/outpatient-inventory` | 可删，只留想法 | MAUI/WebAPI 门诊库存管理，但 API 仍是早期空实现 |
| `feature/programcell` | 可删，只留想法 | Blazor GitLab/Jenkins 看板，适合在 `yf.pt` 重写 |
| `feature/tools-components` | 归档或删除 | WinForms 小工具集合 |
| `feature/universal-invoice` | 删除 | 只有文档/README，没有项目文件 |
| `feature/yunwei-tool` | 可删，只留想法 | Blazor Server 运维/项目工具，含生成物，模型可参考 |

## 建议下一步

1. 推送已确认的本地提交：
   - `2025CodeRepository`：推送清理提交。
   - `Feishu.Context`：推送已合并后的 `main`。
   - `yf.pt`：推送已合并后的 `main`。
2. `MegaRepo`：不再优先拆分支；`archive/localdts` 以独立私有仓 `LocalDts` 为准，其余分支按文档留想法即可。
3. 待你明确确认后可清理的私有仓：
   - `vibe-coding-platform`
   - `chatbot`
   - `autoTicket`

## 已做验证

- 浅克隆缺失的公开仓库，并拉到远端分支引用。
- 列出所有本地仓库和远端分支。
- 确认所有工作区都是 clean。
- 检查项目文件、README、分支文档和代表性源码树。
- 确认 `Jvedio` 的 `kvdeio`、`dev-5.0` 分支存在，并检查与 `master` 的差异规模；已形成 `JVEDIO_BRANCH_NOTE.zh-CN.md`。
- 通过 GitHub CLI 读取到 23 个仓库，其中 9 个私有仓；已对当前可见仓库形成远端分支快照。
- 确认 `DataForge.Core` 的 `trae` 分支已被 `main` 吸收；确认 `PITS` 两个 `trae` 分支已被 `codex/merge-pits-branches` 吸收。
- 已删除 `SvgManager`、`avalonia-browser`、`autoCrafter`、`VibeSkills` 的本地目录，远端未删除。
- 已合并 `yf.pt` 的真实代码分支，并执行 `dotnet restore` / `dotnet build`，构建通过。
- 做过轻量密钥关键词扫；未发现明确真实泄漏，主要是普通变量名、公钥 token、依赖元数据等误报。

没有对新克隆仓库跑完整 restore/build/test。等你确定要保留哪些，再跑构建更划算。
