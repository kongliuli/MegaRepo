# SpaceSniffMax 分支留档

日期：2026-07-02
仓库：`D:\Code\githubDown\SpaceSniffMax`

## 结论

`SpaceSniffMax` 不应删除。

它的默认远端 HEAD 指向 `origin/main`，而 `origin/main` 内容很薄，容易误判为空仓。但真正完整的项目在 `origin/master`，该分支包含完整的 Tauri 2 + Rust + React 桌面磁盘空间分析器。

后续整理时建议：

1. 保留仓库。
2. 以 `origin/master` 作为有效主线判断。
3. 如需长期维护，先决定是否把 `master` 合并或迁移到 `main`，再调整 GitHub 默认分支。

## 分支情况

| 分支 | 判断 | 说明 |
| --- | --- | --- |
| `origin/main` | 不作为主线依据 | 默认 HEAD，但内容很薄，只能说明初始化状态 |
| `origin/master` | 保留 | 完整项目，最近提交为 MFT 扫描、缓存、树构建和 benchmark 相关修复 |

## 项目定位

桌面磁盘空间分析器，方向接近 SpaceSniffer / WizTree，但技术栈更现代：

- 桌面壳：Tauri 2
- 后端：Rust
- 前端：React + TypeScript
- 可视化：D3 / Recharts
- 状态管理：Zustand
- 构建：Vite / pnpm

README 中标注版本为 `v0.9.10`，状态为功能完整、预发布打磨阶段。

## 核心功能

- 并行目录遍历：基于 `jwalk`。
- NTFS MFT 加速扫描：Windows 下读取 Master File Table，失败时回退普通目录遍历。
- 增量扫描缓存：基于缓存做快速重扫。
- 硬链接和 junction 处理：避免重复统计。
- D3 TreeMap：可交互缩放和下钻。
- 大文件列表：可过滤和排序。
- 重复文件检测：名称、大小、内容 hash 多层判断。
- AI 清理建议：DeepSeek API + 离线启发式兜底。
- 文件分类：Magika ONNX + 规则分类。
- 安全删除：移动到回收站。
- 导出：CSV、JSON、HTML、SVG、PNG、文本树、剪贴板。
- CLI：`spacesniff-cli` 可无界面扫描。

## 关键路径

| 路径 | 价值 |
| --- | --- |
| `README.md` | 完整项目说明、功能清单、架构摘要 |
| `docs/ARCHITECTURE.md` | 架构文档 |
| `docs/PROJECT-STATUS.md` | 项目状态 |
| `docs/TASK-BREAKDOWN.md` | 任务拆解 |
| `src-tauri/src/scanner/mft_scanner.rs` | Windows NTFS MFT 加速扫描核心 |
| `src-tauri/src/scanner/walker.rs` | 普通目录遍历 |
| `src-tauri/src/cache` | 增量扫描缓存 |
| `src-tauri/src/ai` | AI 建议、预算、离线兜底 |
| `src-tauri/src/classifier` | 文件分类 |
| `src-tauri/src/filter` | 自定义过滤 DSL |
| `src-tauri/src/delete/mod.rs` | 安全删除 |
| `src-tauri/src/export/mod.rs` | 多格式导出 |
| `src-tauri/src/bin/cli.rs` | CLI 扫描器 |
| `src/views` | 前端主要视图 |
| `src/components/charts` | TreeMap、Sunburst、Pie 等可视化组件 |
| `src/store` | Zustand 状态切片 |

## 后续建议

最小整理路径：

1. `git checkout master`
2. 跑一次 `pnpm install` / `pnpm tauri dev` 验证能否启动。
3. 如果要继续维护，把 GitHub 默认分支改为 `master`，或把 `master` 合并进 `main`。
4. 再补一个简短 README 说明：`main` 是空壳历史，`master` 才是完整实现。

不要在没有迁移 `origin/master` 的情况下删除本仓。
