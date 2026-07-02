# 私有仓库盘点

日期：2026-07-02
账号：`kongliuli`

## GitHub CLI 结果

通过 `gh repo list kongliuli --visibility private` 看到 9 个私有仓库：

| 仓库 | 语言 | 最近推送 | 本地状态 | 结论 |
| --- | --- | --- | --- | --- |
| `SpaceSniffMax` | - | 2026-06-30 | 已克隆 | 保留，以 `origin/master` 为有效主线 |
| `LocalDts` | C# | 2026-05-06 | 已克隆 | 保留，价值高 |
| `vibe-coding-platform` | TypeScript | 2026-04-21 | 已克隆 | 模板/参考，已留档，可删 |
| `chatbot` | TypeScript | 2026-04-21 | 已克隆 | 模板/参考，已留档，可删 |
| `autoTicket` | TypeScript | 2026-04-18 | 已克隆 | 自动票务方向，已留档，可删 |
| `2025CodeRepository` | C# | 2026-04-13 | 已存在 | 已精简留档 |
| `TFTAssistant` | C# | 2026-04-13 | 已克隆 | 游戏助手，按兴趣决定 |
| `xinglin-core` | C# | 2026-02-15 | 已克隆 | 与杏林主线相关，保留 |
| `xinlingMain` | C# | 2025-12-30 | 已克隆 | 杏林旧包/附件仓，先归档 |

## 逐仓判断

### `SpaceSniffMax`

分支：

- `origin/main`：默认 HEAD，内容很薄，只有基础说明，不能代表项目真实状态。
- `origin/master`：完整项目，约 124 个文件，包含 Tauri 2 + Rust + React 桌面磁盘空间分析器。

定位：AI 辅助的桌面磁盘空间分析器，功能接近 SpaceSniffer / WizTree 方向，但加入了现代前端、Rust 后端和 AI 清理建议。

核心内容：

- Rust 后端：`src-tauri/src/scanner`、`cache`、`ai`、`classifier`、`filter`、`delete`、`export`、`tag`、`platform`。
- Windows 加速扫描：`src-tauri/src/scanner/mft_scanner.rs`，NTFS MFT 直接读取，失败时回退目录遍历。
- 前端：React + TypeScript + D3 + Tailwind，包含 Dashboard、TreeMap、AI、BigItems、Settings、CleanupBasket 等视图。
- CLI：`src-tauri/src/bin/cli.rs`，可做无界面扫描输出。
- 文档：`docs/ARCHITECTURE.md`、`docs/PROJECT-STATUS.md`、`docs/TASK-BREAKDOWN.md`、`docs/CHANGELOG.md`。

判断：

- 之前“几乎空”的判断只看到了默认 `origin/main`，是不完整结论。
- `origin/master` 内容很全，且项目定位清晰，代码和文档都完整。
- 不应进入删除候选。

结论：保留。后续如果整理本地分支，建议先把 `master` 内容合并或迁移到主线，再考虑默认分支是否要从 `main` 调整为 `master`。

详见：`SPACESNIFFMAX_BRANCH_NOTE.zh-CN.md`。

### `LocalDts`

文件数：328
项目数：38
测试相关文件：22
分支：`origin/main`、`origin/trae/solo-agent-v64RLs`

定位：.NET 数据迁移工具，支持多数据源、多目标源和插件化转换。

判断：

- 这是 `MegaRepo/archive/localdts` 的独立私有仓版本。
- 价值高于 `MegaRepo` 内部归档分支。
- `origin/trae/solo-agent-v64RLs` 增加 checkpoint/security/tests 等内容，后续整理时值得合并或摘取。
- 应以后续维护 `LocalDts` 私有仓为主，而不是从 `MegaRepo` 恢复。

结论：保留。

详见：`LOCALDTS_RETENTION_NOTE.zh-CN.md`。

### `vibe-coding-platform`

文件数：116
项目文件：`package.json`

定位：Vercel 风格的 AI coding platform 模板，包含 prompt -> sandbox -> live preview -> file explorer -> command logs 的端到端应用思路。

技术栈：

- Next.js
- AI SDK
- Vercel AI Gateway
- Vercel Sandbox
- Tailwind CSS
- shadcn/ui

判断：更像模板/参考实现，不像你自己的长期产品主线。

结论：已写复活说明，可删除本体。

详见：`VIBE_CODING_PLATFORM_ARCHIVE_NOTE.zh-CN.md`。

### `chatbot`

文件数：173
项目文件：`package.json`
测试相关文件：10

定位：Next.js + AI SDK chatbot 模板，含 Auth、数据库、文件存储、Playwright 等。

判断：更像上游 AI chatbot 模板，参考价值在架构和组件组织，不适合长期保留整个仓。

结论：已写复活说明，可删除本体。

详见：`CHATBOT_ARCHIVE_NOTE.zh-CN.md`。

### `autoTicket`

文件数：59
项目文件：

- `package.json`
- `autoTicket.sln`
- `AutoTicket.Core`
- `AutoTicket.Cli`
- `AutoTicket.Config`
- `AutoTicket.Logging`
- `AutoTicket.Platforms`

分支：

- `origin/main`
- `origin/trae/solo-agent-4wA9Gt`
- `origin/trae/solo-agent-9WDg6n`
- `origin/trae/solo-agent-MHmYQC`
- `origin/trae/solo-agent-vYyGSS`
- `origin/trae/solo-agent-yczxWm`

判断：

- 这是自动票务抢购方向。
- 和之前 `AutoTicket` / `AutoWebClient` 的清理判断一致：风险和维护成本高。
- 有 TypeScript 前端和 C# 核心/CLI/配置/日志拆分，架构可记录，但不建议继续维护。

结论：已写复活说明，可删除本体。`origin/trae/solo-agent-4wA9Gt` 是最值得复活参考的分支。

详见：`AUTOTICKET_ARCHIVE_NOTE.zh-CN.md`。

### `TFTAssistant`

文件数：138
项目数：5
测试相关文件：22

定位：Teamfight Tactics 游戏助手，包含 Core、App、Overwolf、测试项目。

判断：

- 项目结构完整。
- 有测试。
- 是否保留取决于你还玩不玩/还做不做 TFT 助手。

结论：暂不删。后续如果不做游戏助手，再写复活说明后删。

详见：`TFTASSISTANT_RETENTION_NOTE.zh-CN.md`。

### `xinglin-core`

文件数：163
项目较多，包含：

- Models
- DataAdapters
- LicenseLib
- LicenseConsole
- UI tests
- Integration tests
- xinglin-core 主项目

定位：杏林核心库/控件箱/模板树/授权/适配器等核心能力。

判断：

- 与活跃项目 `xinglin`、`ReportPlatform` 关系密切。
- 有测试和集成测试。
- 不应归入删除候选。

结论：保留。

详见：`XINGLIN_CORE_RETENTION_NOTE.zh-CN.md`。

### `xinlingMain`

文件数：152
项目数：5
附件压缩包：3 个，包括 `.7z` / `.rar`

内容：

- `Xinglin/Xinglin.sln`
- `XingLinMain`
- `MedicalFormApp`
- `HandyControlsLibrary`
- `testwpf`
- `RePoeDataCatcher.7z`
- `TorrentDownloader.rar`
- `可拖拽的报告单编辑.rar`

判断：

- 看起来是杏林旧主仓或旧实验集合。
- 含压缩包附件，维护性差。
- 因为和杏林有关，不能直接删。

结论：先归档，不删。后续确认 `xinglin` / `xinglin-core` 已覆盖后，再写复活说明或迁移残留附件。

详见：`XINLINGMAIN_ARCHIVE_NOTE.zh-CN.md`。

## 推荐动作

1. 保留：
   - `SpaceSniffMax`，以 `origin/master` 为准
   - `LocalDts`
   - `xinglin-core`
   - `TFTAssistant` 暂保留
   - `xinlingMain` 暂归档
2. 写复活说明后删除：
   - `vibe-coding-platform`
   - `chatbot`
   - `autoTicket`
3. 不再从 `MegaRepo/archive/localdts` 恢复 LocalDts，以私有仓 `LocalDts` 为准。
4. 分支验证详见：`PRIVATE_BRANCH_AUDIT.zh-CN.md`。
