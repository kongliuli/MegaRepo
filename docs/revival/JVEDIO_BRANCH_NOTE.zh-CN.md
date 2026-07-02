# Jvedio 分支保留说明

日期：2026-07-02
目录：`D:\Code\githubDown\Jvedio`

## 结论

`Jvedio` 不应该进入删除队列。

原因不是默认分支本身多完整，而是远端存在两个对后续本地开发有价值的分支：

- `origin/dev-5.0`：更像 v5/refactor 主线候选。
- `origin/kvdeio`：更像图片收藏、NAS、ZSpace 方向的实验分支。

没有发现字面名为 `v-5` 的远端分支；如果后续提到 v5，当前按 `origin/dev-5.0` 理解。

## 分支概览

| 分支 | 最新提交 | 判断 |
| --- | --- | --- |
| `origin/master` | `0d29c1d 新增图片模式 (todo)` | 当前默认观察基线 |
| `origin/dev-5.0` | `61a580e Remove accidental nested Jvedio submodule stub` | 保留，作为 v5/refactor 主线候选 |
| `origin/kvdeio` | `a3c8e43 Add user picture collections and ZSpace NAS research (Phase D)` | 保留，作为图片/NAS/ZSpace 实验分支 |

## `origin/dev-5.0`

相对 `origin/master` 约 261 个文件变更，主要价值在：

- `Jvedio-WPF/Jvedio/Core/Metadata/`：元数据提供器、缓存、NFO/HTTP 查询、metadata engine。
- `Jvedio-WPF/Jvedio/Core/Scan/`：扫描发现、hash path、video/picture/game pipeline 拆分。
- `Jvedio-WPF/Jvedio/Core/UI/`：媒体 UI host、侧边导航、筛选查询、列表模式。
- `Jvedio-WPF/Jvedio/Window*/`、`VideoList.*`：把大 code-behind 拆成 partial 文件。
- `Jvedio-WPF/Jvedio.Test/`：metadata、scan、UI smoke 等测试。
- `Jvedio-WPF/docs/`：包含 ADR 和重构说明。

风险点：

- 分支里有 `.bak` / `.bak2` 和临时恢复脚本。
- 分支里有 `nuget.exe` 和部分二进制 DLL。
- 如果后续要合并，先清理生成物和备份文件，再处理代码。

## `origin/kvdeio`

相对 `origin/master` 约 292 个文件变更。它包含大量 `dev-5.0` 的重构痕迹，但不是简单的上层叠加。

额外价值主要在：

- `Core/Scan/NasPathHelper.cs`
- `Core/Scan/PicturePathHelper.cs`
- `Core/Scan/PictureScanIndexBuilder.cs`
- `Core/Media/PictureRemoteImageService.cs`
- `Core/Media/PictureThumbnailHelper.cs`
- `Core/UI/PictureCollectionService.cs`
- `Core/UI/PictureFolderTreeService.cs`
- `Core/UI/PictureListQuery.cs`
- `VideoList.PictureCollection.partial.cs`
- `Models/Entities/PictureCollection*`、`PictureFile`、`PictureFolderNode`
- `Jvedio-WPF/docs/adr/NAS-001-zspace-research.md`
- `PictureScanIndexBuilderTest`、`PictureBrowseUiTest`、`PictureCollectionTest`

风险点：

- 该分支删除或弱化了部分 game/comic 相关代码，不适合无脑合并到 `dev-5.0` 或 `master`。
- 同样含有备份文件、二进制文件、临时脚本。

## 后续本地开发建议

1. 本地长期保留整个 `Jvedio` 仓库。
2. 新建开发分支时，从明确目标分支切出：
   - v5/refactor 工作：从 `origin/dev-5.0` 切。
   - 图片/NAS/ZSpace 工作：从 `origin/kvdeio` 切。
3. 不要用默认分支完整度判断是否删除。
4. 不要直接把 `kvdeio` 合进 `dev-5.0`；先挑功能点，再清理备份文件和二进制。

## 清理判断

`Jvedio` 当前保留价值大于占用空间成本。它属于“分支内容影响后续判断”的仓库，应该保留到本轮仓库精简结束之后再决定是否拆分或归档。
