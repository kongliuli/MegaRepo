# PITS 分支保留说明

日期：2026-07-02
目录：`D:\Code\githubDown\PITS-Personal-Itinerary-Tracking-System`

## 结论

`PITS-Personal-Itinerary-Tracking-System` 保留。

当前本地分支 `codex/merge-pits-branches` 已经整合两个 `trae` 分支，是后续本地开发入口。

## 分支关系

| 分支 | 最新提交 | 判断 |
| --- | --- | --- |
| `origin/main` | 基线分支 | 不作为当前开发入口 |
| `origin/trae/solo-agent-B7FdSg` | `c115564 docs: 更新 mvp-completion checklist 反映真实闭环状态` | 已被 `codex/merge-pits-branches` 吸收 |
| `origin/trae/solo-agent-SrlJMZ` | MVP art / SVP art 方向 | 已被 `codex/merge-pits-branches` 吸收 |
| `origin/codex/merge-pits-branches` | `ab84d3a fix: remove spatialite dependency on android` | 保留，后续开发入口 |

验证：

- `origin/trae/solo-agent-B7FdSg` 是 `codex/merge-pits-branches` 的祖先。
- `origin/trae/solo-agent-SrlJMZ` 是 `codex/merge-pits-branches` 的祖先。
- 本地 `codex/merge-pits-branches` 与 `origin/codex/merge-pits-branches` 同步，工作区干净。

## 主要价值

`codex/merge-pits-branches` 相对 `origin/main` 有大量真实功能增量：

- MAUI MVP App 可运行性修复：Windows、Android、SQLite 初始化、AppData SQLite 路径。
- 个人行程规划闭环：planned / actual trip 对比、email import、restore path。
- 地理轨迹能力：Google Takeout 导入、Stay/Trip 自动分类、出行方式检测、速度着色轨迹。
- 图片和回忆能力：照片地理信息整合、去年今日行程回顾。
- 隐私和运维能力：backup、privacy export、reminder、tracking profile。
- UI 增强：modern dark UI、tab load error 防崩、替换不稳定 map tab 为 trip overview。
- 文档和设计稿：`docs/`、`.trae/specs/`、`mvp-art/`、`svp-art/`。

## 风险点

- 分支里有 `packages-microsoft-prod.deb` 二进制包，后续整理时应删除或改为文档说明安装方式。
- `.trae/specs/` 更适合归档为说明，不一定长期留在主线。
- 这是开发入口，不等于已经可以直接发布；后续开发前仍需跑 MAUI/.NET 构建验证。

## 清理判断

不要删除仓库。后续如果只保留一个入口，优先保留 `codex/merge-pits-branches`，再考虑把它合入 `main` 或把 GitHub 默认分支切换到该成果线。
