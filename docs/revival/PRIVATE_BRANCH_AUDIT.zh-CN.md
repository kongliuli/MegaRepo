# 私有仓库分支盘点

日期：2026-07-02

## 结论

已刷新远端引用并逐分支检查。当前最重要的纠偏是：

- `SpaceSniffMax`：默认 `origin/main` 很薄，但完整项目在 `origin/master`，保留。
- `LocalDts`：`origin/trae/solo-agent-v64RLs` 有有效增强，保留观察。
- `autoTicket`：多分支都看过，整体仍建议留档后删除；其中 `origin/trae/solo-agent-4wA9Gt` 最有复活参考价值。

## 分支表

| 仓库 | 分支 | 文件/项目/测试 | 判断 |
| --- | --- | --- | --- |
| `SpaceSniffMax` | `origin/main` | 4 / 0 / 0 | 空壳，不作为主线依据 |
| `SpaceSniffMax` | `origin/master` | 124 / 4 / 0 | 完整 Tauri/Rust/React 磁盘分析器，保留 |
| `LocalDts` | `origin/main` | 381 / 38 / 7 | 主线，保留，见 `LOCALDTS_RETENTION_NOTE.zh-CN.md` |
| `LocalDts` | `origin/trae/solo-agent-v64RLs` | 391 / 38 / 9 | 有 checkpoint/security/tests 等增强，保留观察 |
| `vibe-coding-platform` | `origin/main` | 117 / 2 / 0 | Vercel AI coding 模板，留档后可删 |
| `chatbot` | `origin/main` | 178 / 2 / 9 | Vercel AI SDK chatbot 模板，留档后可删 |
| `autoTicket` | `origin/main` | 114 / 16 / 0 | 前端 + C# 核心 + node_modules，留档后可删 |
| `autoTicket` | `origin/trae/solo-agent-4wA9Gt` | 52 / 2 / 13 | 删除了前端和 node_modules，转向本地服务/测试，最值得留档 |
| `autoTicket` | `origin/trae/solo-agent-9WDg6n` | 119 / 16 / 1 | 架构评审向，保留文档价值即可 |
| `autoTicket` | `origin/trae/solo-agent-MHmYQC` | 76 / 14 / 0 | README/文档整理向 |
| `autoTicket` | `origin/trae/solo-agent-vYyGSS` | 7 / 0 / 0 | 只有检查报告类内容，不保留 |
| `autoTicket` | `origin/trae/solo-agent-yczxWm` | 114 / 16 / 0 | WebUI 配置管理向，价值不高 |
| `2025CodeRepository` | `origin/main` | 1271 / 169 / 0 | 远端原始大仓；本地已精简，不再按远端复原 |
| `2025CodeRepository` | `origin/trae/solo-agent-zK4A1G` | 1273 / 169 / 0 | 比 main 多整理文档，价值已被本地留档覆盖 |
| `TFTAssistant` | `origin/main` | 155 / 5 / 20 | 完整、有测试，暂不删，见 `TFTASSISTANT_RETENTION_NOTE.zh-CN.md` |
| `xinglin-core` | `origin/master` | 191 / 18 / 11 | 杏林核心相关，保留，见 `XINGLIN_CORE_RETENTION_NOTE.zh-CN.md` |
| `xinlingMain` | `origin/main` | 153 / 5 / 0 | 杏林旧主仓/附件仓，先归档不删，见 `XINLINGMAIN_ARCHIVE_NOTE.zh-CN.md` |

## 删除前优先看

如果后续要复活，不要从整个仓库恢复，直接看这些点：

- `SpaceSniffMax`：`origin/master`
- `LocalDts`：`origin/main` + `origin/trae/solo-agent-v64RLs`
- `autoTicket`：`origin/trae/solo-agent-4wA9Gt`
