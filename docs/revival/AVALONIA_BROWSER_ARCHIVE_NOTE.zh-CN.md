# avalonia-browser 复活说明

日期：2026-07-02
仓库：`D:\Code\githubDown\avalonia-browser`
结论：计划删除；保留本文档用于未来复活。

## 项目定位

`avalonia-browser` 是一个基于 Avalonia 的专用浏览器 POC，目标是访问和解析 `autoglm.zhipuai.cn` 等站点。

它更像“浏览器壳 + SQLite 本地状态”的实验，不是完整产品。

## 当前组成

| 路径 | 价值 |
| --- | --- |
| `src/SimpleWebBrowserDemo` | 主项目，Avalonia 桌面应用 |
| `src/SimpleWebBrowserDemo/Services` | 导航、网页、书签、历史记录、用户设置、数据库服务 |
| `src/SimpleWebBrowserDemo/Repositories` | SQLite/Dapper 仓储接口和实现 |
| `src/SimpleWebBrowserDemo/Assets/init.sql` | 本地 SQLite 初始化脚本 |
| `docs/20260119` | 技术选型和功能设计讨论 |
| `docs/TodoList` | 从极简 demo 到组件化基座的 TODO |

## 技术栈

- .NET 8
- Avalonia 11.1
- CommunityToolkit.Mvvm
- Dapper
- Microsoft.Data.Sqlite

## 可复用点

未来如果要复活，只取这几块：

- Avalonia 桌面壳：`App.axaml`、`Program.cs`、`Views/MainWindow.axaml`
- 本地浏览状态模型：`Models/BrowserState.cs`
- 书签/历史记录模型和仓储：`Models/*Bookmark*`、`Repositories/*Bookmark*`、`Repositories/*History*`
- SQLite 初始化：`Assets/init.sql`
- 用户设置服务：`Services/UserSettingsService.cs`

## 不保留原因

1. 这是 POC，不是稳定产品线。
2. 当前没有确认还要继续做 `autoglm` 专用桌面壳。
3. `DateTimeConverter.cs` 里仍有 `NotImplementedException`。
4. 文档/TODO 多于实际可用功能。

## 复活路径

如果后续要重做：

1. 新建干净 Avalonia 项目。
2. 只搬 `Models`、`Repositories`、`Services` 中书签/历史/设置相关代码。
3. 复用 `init.sql`。
4. 重新实现网页宿主，不直接整仓恢复。
