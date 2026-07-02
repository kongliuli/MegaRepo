# SvgManager 留档说明

日期：2026-07-02
仓库：`D:\Code\githubDown\SvgManager`
结论：不保留为长期维护仓库。

## 项目定位

`SvgManager` 是一个 SVG 颜色/属性处理工具集合，主要围绕 SVG 读取、颜色归一化、批量处理、存储和预览。

它不是单一清晰产品，而是多个阶段的实现叠在一个仓库中：

- WinForms 桌面工具
- ASP.NET Core API
- Core 类库
- SQLite 初始化工具
- MVP 版本
- 示例 SVG 和测试数据库
- 分析脚本和图表文档

## 项目组成

| 路径 | 类型 | 说明 |
| --- | --- | --- |
| `SvgColorNormalizer.Core` | 类库 | 核心处理逻辑、数据源、存储接口、颜色归一化 |
| `SvgColorNormalizer` | WinForms | 桌面 UI，目标框架 `net10.0-windows` |
| `SvgColorNormalizer.Api` | Web API | API 壳，目标框架 `net10.0` |
| `SvgDbInitializer` | Console | 初始化 SQLite SVG 数据库 |
| `SvgManagerMvp` | WinForms MVP | 早期/并行 MVP 版本，目标框架 `net6.0-windows` |
| `svg_storage` | 示例数据 | `circle.svg`、`star.svg` 等示例 SVG |
| `参考` | 文档 | `SvgColorNormalizer_Development_Guide.md` |

## 核心能力

- 从文件夹、SQLite、SQL Server、PostgreSQL、API 等来源读取 SVG。
- 对 SVG 颜色进行标准化和格式转换。
- 批量处理 SVG。
- 将 SVG 写入文件夹或 SQLite。
- WinForms UI 中预览 SVG、查看/修改属性。
- 提供 API 形式的 SVG normalize/batch normalize 接口雏形。

## 不保留原因

1. 项目边界混乱：同一仓里同时存在 Core、API、两个 WinForms 实现、DB 初始化和脚本。
2. 目标框架混杂：`net6.0`、`net6.0-windows`、`net10.0`、`net10.0-windows` 同时存在。
3. 维护收益不高：除非近期有明确 SVG 数据清洗需求，否则长期维护成本高于价值。
4. API 项目还带默认 `weatherforecast` 示例痕迹，说明工程化收尾不足。
5. MVP 和新版 Core/UI 能力重叠，后续继续维护容易重复修两套。

## 可复用内容

如果未来需要重新做 SVG 工具，优先只取这些内容：

- `SvgColorNormalizer.Core\Core\ColorNormalizer.cs`
- `SvgColorNormalizer.Core\Core\SvgProcessor.cs`
- `SvgColorNormalizer.Core\Core\SvgBatchProcessor.cs`
- `SvgColorNormalizer.Core\DataSources\ISvgSource.cs`
- `SvgColorNormalizer.Core\DataSources\ISvgStorage.cs`
- `SvgManagerMvp\Services\SvgColorConverter.cs`
- `SvgManagerMvp\Services\SvgAttributeService.cs`

## 建议处理

计划删除仓库，但保留本说明作为索引。

如果删除前想再抢救一小块，最小动作是只抽出 `SvgColorNormalizer.Core` 里颜色处理相关类，其他 UI/API/DB 初始化全部不要。
