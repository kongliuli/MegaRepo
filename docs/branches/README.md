# 分支文档总览

本目录包含 MegaRepo 所有分支的详细说明文档。每个分支都有对应的 `BRANCH.md` 文件，记录了该分支的项目信息、技术栈、核心功能等内容。

## 分支分类统计

| 分类 | 数量 | 说明 |
|------|------|------|
| 主分支 | 1 | main - 主分支 |
| 功能分支 | 13 | feature/* - 各功能项目分支 |
| 归档分支 | 2 | archive/* - 已归档项目分支 |
| 内部分支 | 1 | trae/* - Trae AI 工作分支 |
| **总计** | **17** | |

---

## 🎯 主分支

| 分支 | 项目名称 | 技术栈 | 说明文档 |
|------|----------|--------|----------|
| [main](../../README.md) | MegaRepo 超级整合仓库 | - | 主分支，包含分析文档与整合计划 |

---

## 🔧 功能分支 (feature/*)

### 自动化工具

| 分支 | 项目名称 | 技术栈 | 说明文档 |
|------|----------|--------|----------|
| feature/autoticket | AutoTicket 自动购票系统 | .NET 5.0, C#, WinForms | [查看文档](feature-autoticket.md) |
| feature/autowebclient | AutoWebClient 自动化Web客户端 | .NET, C#, WinForms, CEFSharp | [查看文档](feature-autowebclient.md) |
| feature/dingtalklib | DingTalkLib 钉钉接口封装库 | .NET 6.0, C# | [查看文档](feature-dingtalklib.md) |
| feature/diting-utils | Diting.Utils 通用工具库 | .NET 8.0, C#, Newtonsoft.Json | [查看文档](feature-diting-utils.md) |

### WPF 桌面应用

| 分支 | 项目名称 | 技术栈 | 说明文档 |
|------|----------|--------|----------|
| feature/fileexplorer | FileExplorer 文件资源管理器 | .NET 8.0, C#, WPF, Material Design | [查看文档](feature-fileexplorer.md) |
| feature/dbconnection-wpf | DbConnection.WPF 数据库连接工具 | .NET 8.0, C#, WPF | [查看文档](feature-dbconnection-wpf.md) |

### Blazor Web 应用

| 分支 | 项目名称 | 技术栈 | 说明文档 |
|------|----------|--------|----------|
| feature/programcell | ProgramCell Blazor项目管理工具 | .NET 8.0, C#, Blazor, AntDesign, Docker | [查看文档](feature-programcell.md) |
| feature/blazor-trae | Blazor.Trae Trae集成Blazor应用 | .NET 8.0, C#, Blazor, Google Maps | [查看文档](feature-blazor-trae.md) |
| feature/yunwei-tool | YunweiTool 运维工具平台 | .NET 8.0, C#, Blazor Server | [查看文档](feature-yunwei-tool.md) |

### 业务系统

| 分支 | 项目名称 | 技术栈 | 说明文档 |
|------|----------|--------|----------|
| feature/universal-invoice | UniversalInvoice 通用票据处理工具 | C# .NET 8, MAUI/WPF, SQLite | [查看文档](feature-universal-invoice.md) |
| feature/outpatient-inventory | OutpatientInventoryManager 门诊库存管理系统 | C#, MAUI, EF Core | [查看文档](feature-outpatient-inventory.md) |

### 工具组件

| 分支 | 项目名称 | 技术栈 | 说明文档 |
|------|----------|--------|----------|
| feature/tools-components | Tools Components 工具组件合集 | .NET 6.0, C#, WinForms | [查看文档](feature-tools-components.md) |

---

## 📦 归档分支 (archive/*)

| 分支 | 项目名称 | 技术栈 | 说明文档 | 归档原因 |
|------|----------|--------|----------|----------|
| archive/localdts | LocalDts 数据迁移工具 | .NET 8.0, C#, WPF, EF Core | [查看文档](archive-localdts.md) | 项目已完成或暂停 |
| archive/mindmap | MindMap 思维导图工具 | Web/JavaScript | [查看文档](archive-mindmap.md) | 项目已完成或暂停 |

---

## 🤖 内部分支 (trae/*)

| 分支 | 项目名称 | 技术栈 | 说明文档 |
|------|----------|--------|----------|
| trae/solo-agent-edXDXQ | Trae Solo Agent 工作分支 | 自动化脚本 | [查看文档](trae-solo-agent-edXDXQ.md) |

---

## 技术栈统计

### 按框架分类
- **.NET 8.0**: 6个项目 (diting-utils, fileexplorer, dbconnection-wpf, programcell, blazor-trae, yunwei-tool)
- **.NET 6.0**: 2个项目 (dingtalklib, tools-components)
- **.NET 5.0**: 1个项目 (autoticket)
- **MAUI**: 1个项目 (outpatient-inventory)
- **Blazor**: 3个项目 (programcell, blazor-trae, yunwei-tool)
- **WPF**: 2个项目 (fileexplorer, dbconnection-wpf)
- **WinForms**: 3个项目 (autoticket, autowebclient, tools-components)

### 按语言分类
- **C#**: 13个项目 (绝大多数)
- **JavaScript**: 1个项目 (mindmap)

---

## 来源仓库分布

| 来源仓库 | 分支数量 | 分支列表 |
|----------|----------|----------|
| 2025CodeRepository | 8 | autoticket, autowebclient, dingtalklib, diting-utils, programcell, blazor-trae, yunwei-tool, dbconnection-wpf, fileexplorer |
| Practice-Projects-Hub | 2 | universal-invoice, tools-components |
| NewRepoBySiHuo | 1 | outpatient-inventory |
| LocalDts | 1 | archive/localdts |
| MindMap | 1 | archive/mindmap |
| moyu | 1 | tools-components |
| MegaRepo (内部) | 1 | trae/solo-agent-edXDXQ |

---

## 文档更新说明

- 各分支的详细文档存放在各分支的 `BRANCH.md` 文件中
- 本目录的文档为各分支文档的镜像副本，用于在主分支中集中查阅
- 如需更新文档，请在对应分支修改 `BRANCH.md` 后同步到本目录
