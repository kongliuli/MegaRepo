# TFT Assistant 开发文档

> 基于 Hearthstone Deck Tracker (HDT) 架构思想，使用 Riot Live Client Data API + Overwolf SDK 构建的云顶之弈个人辅助工具。

---

## 目录

- [1. 项目概述](#1-项目概述)
- [2. 技术栈](#2-技术栈)
- [3. 项目结构](#3-项目结构)
- [4. 环境搭建](#4-环境搭建)
- [5. 核心层设计 (TFTAssistant.Core)](#5-核心层设计)
  - [5.1 游戏状态模型](#51-游戏状态模型)
  - [5.2 数据获取抽象](#52-数据获取抽象)
  - [5.3 推荐引擎](#53-推荐引擎)
  - [5.4 事件总线](#54-事件总线)
  - [5.5 持久化](#55-持久化)
- [6. 数据层实现](#6-数据层实现)
  - [6.1 Live Client Data API 客户端](#61-live-client-data-api-客户端)
  - [6.2 Overwolf 事件适配器](#62-overwolf-事件适配器)
  - [6.3 Data Dragon 静态数据](#63-data-dragon-静态数据)
  - [6.4 Riot API 赛后数据](#64-riot-api-赛后数据)
  - [6.5 组合数据源](#65-组合数据源)
- [7. 推荐引擎实现](#7-推荐引擎实现)
  - [7.1 阵容匹配器](#71-阵容匹配器)
  - [7.2 装备建议器](#72-装备建议器)
  - [7.3 经济顾问](#73-经济顾问)
  - [7.4 强化符文评分](#74-强化符文评分)
- [8. Overwolf 集成层](#8-overwolf-集成层)
  - [8.1 应用清单 manifest.json](#81-应用清单-manifestjson)
  - [8.2 C# ↔ JS 桥接](#82-c--js-桥接)
  - [8.3 窗口管理](#83-窗口管理)
- [9. 前端 UI 设计](#9-前端-ui-设计)
  - [9.1 覆盖层 UI](#91-覆盖层-ui)
  - [9.2 桌面主窗口](#92-桌面主窗口)
  - [9.3 前端状态管理](#93-前端状态管理)
- [10. 数据库设计](#10-数据库设计)
- [11. 依赖注入与启动流程](#11-依赖注入与启动流程)
- [12. 模块依赖关系](#12-模块依赖关系)
- [13. Riot 合规指南](#13-riot-合规指南)
- [14. 开发路线图](#14-开发路线图)
- [15. 测试策略](#15-测试策略)
- [16. 部署与分发](#16-部署与分发)
- [附录 A: Live Client API 端点参考](#附录-a-live-client-api-端点参考)
- [附录 B: Overwolf TFT 事件参考](#附录-b-overwolf-tft-事件参考)
- [附录 C: Data Dragon 数据结构](#附录-c-data-dragon-数据结构)
- [附录 D: HDT 架构映射对照表](#附录-d-hdt-架构映射对照表)

---

## 1. 项目概述

### 1.1 项目目标

构建一个个人使用的云顶之弈 (Teamfight Tactics) 辅助工具，实现以下核心能力：

1. **游戏状态追踪**：实时追踪棋盘、备战席、商店、经济状态（类似 HDT 的记牌器功能）
2. **智能建议**：阵容匹配、装备分配、经济策略、强化符文评分（遵守 Riot 政策）
3. **赛后分析**：对局历史记录、胜率统计、阵容表现分析

### 1.2 设计原则

- **仿 HDT 架构**：采用 HDT 的分层解耦模式（数据采集 → 状态构建 → 业务逻辑 → UI 渲染）
- **接口驱动**：所有模块通过接口解耦，可独立测试和替换
- **合规优先**：严格遵守 Riot 第三方工具政策，所有功能在合规边界内设计
- **渐进式开发**：从 MVP 开始，按阶段迭代

### 1.3 与 HDT 的核心差异

| 维度 | HDT（炉石） | TFT Assistant |
|------|------------|---------------|
| 数据获取 | 解析日志文件 `Power.log` | HTTP 请求 `localhost:2999` |
| 数据格式 | 正则匹配日志行（脆弱） | JSON API（稳定） |
| 辅助数据 | DLL 注入读内存 (HearthMirror) | Overwolf 事件 API |
| 覆盖层 | WPF 透明窗口 + Win32 API | Overwolf SDK 内置透明窗口 |
| 合规风险 | 低（暴雪宽松） | 中（Riot 严格） |
| 推荐功能 | 无（记牌即可） | 阵容/装备/经济/符文建议 |
| 数据库 | XML 文件 | SQLite |

---

## 2. 技术栈

| 组件 | 技术选型 | 版本 | 说明 |
|------|----------|------|------|
| 运行时 | .NET | 8.0 LTS | 长期支持版本 |
| 语言 | C# | 12.0 | 最新语言特性 |
| Overwolf SDK | overwolf npm package | 最新 | JS 端事件监听 |
| 前端框架 | 原生 HTML/CSS/JS | - | Overwolf 窗口内运行，无需重型框架 |
| HTTP 客户端 | HttpClient | 内置 | 调用 Live Client API |
| JSON 序列化 | System.Text.Json | 内置 | 高性能 JSON 处理 |
| 数据库 | SQLite | Microsoft.Data.Sqlite | 轻量本地存储 |
| 日志 | Serilog | 3.x | 结构化日志 |
| DI 容器 | Microsoft.Extensions.DependencyInjection | 内置 | 依赖注入 |
| 单元测试 | xUnit + Moq | 最新 | 测试框架 |
| 自动更新 | Squirrel.Windows | 2.x | 或 GitHub Releases 自实现 |

---

## 3. 项目结构

```
TFTAssistant/
│
├── src/
│   ├── TFTAssistant.Core/                    # 核心逻辑层（纯 .NET，无 UI 依赖）
│   │   ├── Abstractions/                     # 所有接口定义
│   │   │   ├── IGameDataSource.cs
│   │   │   ├── IGameDataProvider.cs
│   │   │   ├── IStaticDataProvider.cs
│   │   │   ├── IMatchHistoryProvider.cs
│   │   │   ├── IRecommendationEngine.cs
│   │   │   ├── ICompMatcher.cs
│   │   │   ├── IItemAdvisor.cs
│   │   │   ├── IEconomyAdvisor.cs
│   │   │   ├── IAugmentAdvisor.cs
│   │   │   ├── IEventBus.cs
│   │   │   ├── IGameStateRepository.cs
│   │   │   └── IMatchRepository.cs
│   │   ├── Models/                           # 数据模型
│   │   │   ├── Game/
│   │   │   │   ├── GameState.cs
│   │   │   │   ├── BoardUnit.cs
│   │   │   │   ├── BenchUnit.cs
│   │   │   │   ├── ShopUnit.cs
│   │   │   │   ├── PlayerInfo.cs
│   │   │   │   ├── PlayerSummary.cs
│   │   │   │   ├── GameInfo.cs
│   │   │   │   └── Augment.cs
│   │   │   ├── Static/
│   │   │   │   ├── Champion.cs
│   │   │   │   ├── Item.cs
│   │   │   │   ├── ItemComponent.cs
│   │   │   │   ├── Trait.cs
│   │   │   │   ├── MetaComp.cs
│   │   │   │   └── SetVersion.cs
│   │   │   ├── Recommendation/
│   │   │   │   ├── RecommendationSet.cs
│   │   │   │   ├── CompSuggestion.cs
│   │   │   │   ├── ItemSuggestion.cs
│   │   │   │   ├── EconomyHint.cs
│   │   │   │   └── AugmentRating.cs
│   │   │   ├── Analytics/
│   │   │   │   ├── MatchRecord.cs
│   │   │   │   ├── MatchDetail.cs
│   │   │   │   ├── MatchStats.cs
│   │   │   │   └── RoundSnapshot.cs
│   │   │   └── Events/
│   │   │       ├── IGameEvent.cs
│   │   │       ├── GameStarted.cs
│   │   │       ├── GameEnded.cs
│   │   │       ├── BoardChanged.cs
│   │   │       ├── ShopRefreshed.cs
│   │   │       ├── AugmentPicked.cs
│   │   │       ├── PlayerLevelChanged.cs
│   │   │       └── HealthChanged.cs
│   │   ├── Data/                             # 数据获取实现
│   │   │   ├── LiveClientDataProvider.cs
│   │   │   ├── LiveGameDataDto.cs            # API 响应 DTO
│   │   │   ├── OverwolfEventAdapter.cs
│   │   │   ├── CompositeGameDataProvider.cs
│   │   │   ├── DataDragonProvider.cs
│   │   │   ├── RiotApiMatchProvider.cs
│   │   │   └── MockGameDataProvider.cs       # 测试用 Mock
│   │   ├── Engine/                           # 推荐引擎实现
│   │   │   ├── RecommendationEngine.cs
│   │   │   ├── CompMatcher.cs
│   │   │   ├── ItemAdvisor.cs
│   │   │   ├── EconomyAdvisor.cs
│   │   │   ├── AugmentAdvisor.cs
│   │   │   └── ItemBuildDatabase.cs          # 装备推荐规则表
│   │   ├── Events/                           # 事件总线实现
│   │   │   └── InMemoryEventBus.cs
│   │   └── Storage/                          # 持久化实现
│   │       ├── SqliteGameStateRepository.cs
│   │       ├── SqliteMatchRepository.cs
│   │       └── DatabaseInitializer.cs
│   │
│   ├── TFTAssistant.Overwolf/                # Overwolf 集成层
│   │   ├── Bridge/
│   │   │   ├── OWBridge.cs                   # C# ↔ JS 通信桥
│   │   │   └── MessageTypes.cs               # 消息类型定义
│   │   ├── Windows/
│   │   │   └── OWWindowManager.cs            # 窗口生命周期管理
│   │   └── Events/
│   │       └── OWEventDispatcher.cs          # OW 事件分发
│   │
│   └── TFTAssistant.App/                     # 启动入口
│       ├── Bootstrapper.cs                   # DI 注册 + 初始化
│       └── Program.cs
│
├── src/ui/                                   # 前端（Overwolf 窗口 HTML 内容）
│   ├── overlay/                              # 游戏内覆盖层
│   │   ├── index.html
│   │   ├── js/
│   │   │   ├── app.js                        # 入口 + 初始化
│   │   │   ├── bridge-client.js              # 与 C# 后端通信
│   │   │   ├── state-store.js                # 前端状态管理
│   │   │   └── components/
│   │   │       ├── board-panel.js            # 棋盘追踪面板
│   │   │       ├── item-panel.js             # 装备合成面板
│   │   │       ├── comp-panel.js             # 阵容建议面板
│   │   │       ├── econ-panel.js             # 经济提示面板
│   │   │       └── augment-panel.js          # 强化符文面板
│   │   └── css/
│   │       ├── reset.css
│   │       ├── variables.css
│   │       └── overlay.css
│   │
│   └── desktop/                              # 桌面主窗口
│       ├── index.html
│       ├── js/
│       │   ├── app.js
│       │   ├── history.js                    # 对局历史页
│       │   ├── stats.js                      # 统计仪表盘页
│       │   └── settings.js                   # 设置页
│       └── css/
│           ├── reset.css
│           ├── variables.css
│           └── desktop.css
│
├── data/                                     # 静态数据（版本化缓存）
│   └── sets/
│       └── set13/                            # 按赛季组织
│           ├── champions.json
│           ├── items.json
│           ├── traits.json
│           ├── augments.json
│           └── meta-comps.json               # 社区 Meta 阵容数据
│
├── tests/
│   ├── TFTAssistant.Core.Tests/
│   │   ├── Data/
│   │   │   ├── LiveClientDataProviderTests.cs
│   │   │   ├── DataDragonProviderTests.cs
│   │   │   └── CompositeDataProviderTests.cs
│   │   ├── Engine/
│   │   │   ├── CompMatcherTests.cs
│   │   │   ├── ItemAdvisorTests.cs
│   │   │   ├── EconomyAdvisorTests.cs
│   │   │   └── AugmentAdvisorTests.cs
│   │   ├── Events/
│   │   │   └── InMemoryEventBusTests.cs
│   │   └── Storage/
│   │       ├── GameStateRepositoryTests.cs
│   │       └── MatchRepositoryTests.cs
│   └── TFTAssistant.Overwolf.Tests/
│       └── OWBridgeTests.cs
│
├── docs/
│   └── manifest.json                         # Overwolf 应用清单
│
├── TFTAssistant.sln                          # 解决方案文件
├── Directory.Build.props                     # 共享 MSBuild 属性
└── README.md
```

---

## 4. 环境搭建

### 4.1 前置条件

| 工具 | 版本 | 用途 |
|------|------|------|
| .NET 8 SDK | 8.0.x | 后端开发 |
| Visual Studio 2022 / Rider | 最新 | IDE |
| Node.js | 18+ | Overwolf 打包工具 |
| Overwolf 客户端 | 最新 | 运行和调试 |
| Git | 最新 | 版本控制 |
| SQLite | 内置于 .NET | 本地数据库 |

### 4.2 创建解决方案

```bash
# 创建解决方案和项目
dotnet new sln -n TFTAssistant

# 核心层（类库）
dotnet new classlib -n TFTAssistant.Core -o src/TFTAssistant.Core -f net8.0

# Overwolf 集成层（类库，引用 Core）
dotnet new classlib -n TFTAssistant.Overwolf -o src/TFTAssistant.Overwolf -f net8.0

# 启动入口（控制台应用，引用 Overwolf 和 Core）
dotnet new console -n TFTAssistant.App -o src/TFTAssistant.App -f net8.0

# 测试项目
dotnet new xunit -n TFTAssistant.Core.Tests -o tests/TFTAssistant.Core.Tests -f net8.0

# 添加到解决方案
dotnet sln add src/TFTAssistant.Core
dotnet sln add src/TFTAssistant.Overwolf
dotnet sln add src/TFTAssistant.App
dotnet sln add tests/TFTAssistant.Core.Tests

# 添加项目引用
dotnet add src/TFTAssistant.Overwolf reference src/TFTAssistant.Core
dotnet add src/TFTAssistant.App reference src/TFTAssistant.Core
dotnet add src/TFTAssistant.App reference src/TFTAssistant.Overwolf
dotnet add tests/TFTAssistant.Core.Tests reference src/TFTAssistant.Core
```

### 4.3 安装 NuGet 包

```bash
# Core 项目
cd src/TFTAssistant.Core
dotnet add package Microsoft.Data.Sqlite
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
dotnet add package Microsoft.Extensions.Http

# Overwolf 项目
cd ../TFTAssistant.Overwolf
# Overwolf 通过 JS 调用，C# 端不需要额外 NuGet 包

# 测试项目
cd ../../tests/TFTAssistant.Core.Tests
dotnet add package Moq
dotnet add package FluentAssertions
```

### 4.4 Overwolf 开发者账号

1. 访问 [https://developers.overwolf.com/](https://developers.overwolf.com/) 注册开发者账号
2. 创建新应用，获取 App ID
3. 下载 Overwolf 客户端用于本地调试
4. 安装 Overwolf 打包工具：`npm install -g overwolf-packager`

### 4.5 Riot Developer API Key（可选，用于赛后分析）

1. 访问 [https://developer.riotgames.com/](https://developer.riotgames.com/) 注册
2. 生成 Personal API Key（个人项目足够）
3. 记录 API Key，配置在 `appsettings.json` 中

---

## 5. 核心层设计

### 5.1 游戏状态模型

#### 5.1.1 GameState — 游戏状态根对象

对应 HDT 的 `GameV2`，是所有游戏状态的中央仓库。

```csharp
// src/TFTAssistant.Core/Models/Game/GameState.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 游戏状态快照 —— 对应 HDT 的 GameV2
/// 每次从 Live Client API 获取数据后构建一个新实例
/// </summary>
public sealed class GameState : IEquatable<GameState>
{
    public required ActivePlayer ActivePlayer { get; init; }
    public required IReadOnlyList<PlayerSummary> AllPlayers { get; init; }
    public required GameInfo GameInfo { get; init; }
    public required IReadOnlyList<Augment> Augments { get; init; }

    /// <summary>
    /// 快照时间戳（本地时间）
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public bool Equals(GameState? other)
    {
        if (other is null) return false;
        return ActivePlayer.Equals(other.ActivePlayer)
            && GameInfo.Round == other.GameInfo.Round;
    }

    public override int GetHashCode() => HashCode.Combine(ActivePlayer, GameInfo.Round);
}
```

#### 5.1.2 ActivePlayer — 当前玩家状态

```csharp
// src/TFTAssistant.Core/Models/Game/ActivePlayer.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 当前玩家的完整游戏状态
/// </summary>
public sealed class ActivePlayer : IEquatable<ActivePlayer>
{
    public required string SummonerName { get; init; }
    public required int Level { get; init; }
    public required int CurrentGold { get; init; }
    public required double Health { get; init; }
    public required int Experience { get; init; }
    public required int TotalGold { get; init; }
    public required int Placement { get; init; }

    /// <summary>棋盘上的单位</summary>
    public required IReadOnlyList<BoardUnit> Board { get; init; }

    /// <summary>备战席上的单位</summary>
    public required IReadOnlyList<BenchUnit> Bench { get; init; }

    /// <summary>商店中的单位</summary>
    public required IReadOnlyList<ShopUnit> Shop { get; init; }

    public bool Equals(ActivePlayer? other)
    {
        if (other is null) return false;
        return Level == other.Level
            && CurrentGold == other.CurrentGold
            && Health == other.Health
            && Board.SequenceEqual(other.Board)
            && Bench.SequenceEqual(other.Bench);
    }

    public override int GetHashCode() => HashCode.Combine(Level, CurrentGold, Health);
}
```

#### 5.1.3 BoardUnit — 棋盘单位

```csharp
// src/TFTAssistant.Core/Models/Game/BoardUnit.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 棋盘上的一个单位（含位置信息）
/// </summary>
public sealed class BoardUnit : IEquatable<BoardUnit>
{
    /// <summary>英雄名称（如 "Ahri"）</summary>
    public required string ChampionName { get; init; }

    /// <summary>星级（1-3）</summary>
    public required int StarLevel { get; init; }

    /// <summary>装备列表</summary>
    public required IReadOnlyList<string> Items { get; init; }

    /// <summary>棋盘行位置（0-6）</summary>
    public required int Row { get; init; }

    /// <summary>棋盘列位置（0-6）</summary>
    public required int Column { get; init; }

    /// <summary>是否为主 C（由推荐引擎推断）</summary>
    public bool IsMainCarry { get; set; }

    public bool Equals(BoardUnit? other)
    {
        if (other is null) return false;
        return ChampionName == other.ChampionName
            && StarLevel == other.StarLevel
            && Row == other.Row
            && Column == other.Column
            && Items.SequenceEqual(other.Items);
    }

    public override int GetHashCode() => HashCode.Combine(ChampionName, Row, Column);
}
```

#### 5.1.4 BenchUnit — 备战席单位

```csharp
// src/TFTAssistant.Core/Models/Game/BenchUnit.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 备战席上的一个单位
/// </summary>
public sealed class BenchUnit : IEquatable<BenchUnit>
{
    public required string ChampionName { get; init; }
    public required int StarLevel { get; init; }
    public required IReadOnlyList<string> Items { get; init; }
    public required int BenchSlot { get; init; } // 0-8

    public bool Equals(BenchUnit? other)
    {
        if (other is null) return false;
        return ChampionName == other.ChampionName
            && StarLevel == other.StarLevel
            && BenchSlot == other.BenchSlot;
    }

    public override int GetHashCode() => HashCode.Combine(ChampionName, BenchSlot);
}
```

#### 5.1.5 ShopUnit — 商店单位

```csharp
// src/TFTAssistant.Core/Models/Game/ShopUnit.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 商店中的一个可购买单位
/// </summary>
public sealed class ShopUnit : IEquatable<ShopUnit>
{
    public required string ChampionName { get; init; }
    public required int Cost { get; init; } // 1-5 费
    public required int ShopSlot { get; init; } // 0-4

    public bool Equals(ShopUnit? other)
    {
        if (other is null) return false;
        return ChampionName == other.ChampionName && ShopSlot == other.ShopSlot;
    }

    public override int GetHashCode() => HashCode.Combine(ChampionName, ShopSlot);
}
```

#### 5.1.6 PlayerSummary — 其他玩家摘要

```csharp
// src/TFTAssistant.Core/Models/Game/PlayerSummary.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 大厅中其他玩家的摘要信息
/// </summary>
public sealed class PlayerSummary
{
    public required string SummonerName { get; init; }
    public required double Health { get; init; }
    public required int Placement { get; init; }
    public required int TotalGold { get; init; }
    public required int Level { get; init; }
}
```

#### 5.1.7 GameInfo — 对局元信息

```csharp
// src/TFTAssistant.Core/Models/Game/GameInfo.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 对局元信息（回合、阶段等）
/// </summary>
public sealed class GameInfo
{
    public required double GameTime { get; init; }
    public required int Round { get; init; }
    public required int Stage { get; init; } // 1-7 阶段
    public required bool IsPvp { get; init; }
    public required string SetNumber { get; init; } // "Set13"
}
```

#### 5.1.8 Augment — 强化符文

```csharp
// src/TFTAssistant.Core/Models/Game/Augment.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 强化符文
/// </summary>
public sealed class Augment
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int Stage { get; init; } // 第几次选择（1/2/3）
}
```

#### 5.1.9 GameStateDiff — 状态差异

对应 HDT 的 `TagChange` 事件，用于增量更新。

```csharp
// src/TFTAssistant.Core/Models/Game/GameStateDiff.cs
namespace TFTAssistant.Core.Models.Game;

/// <summary>
/// 两次游戏状态之间的差异 —— 对应 HDT 的 TagChange 事件
/// 用于增量更新 UI，避免全量重绘
/// </summary>
public sealed class GameStateDiff
{
    public bool HasBoardChanged { get; init; }
    public bool HasBenchChanged { get; init; }
    public bool HasShopChanged { get; init; }
    public bool HasGoldChanged { get; init; }
    public bool HasHealthChanged { get; init; }
    public bool HasLevelChanged { get; init; }
    public bool HasRoundChanged { get; init; }
    public bool HasAugmentPicked { get; init; }

    public IReadOnlyList<BoardUnit>? NewBoard { get; init; }
    public IReadOnlyList<BenchUnit>? NewBench { get; init; }
    public IReadOnlyList<ShopUnit>? NewShop { get; init; }
    public int? OldGold { get; init; }
    public int? NewGold { get; init; }
    public double? OldHealth { get; init; }
    public double? NewHealth { get; init; }
    public int? OldLevel { get; init; }
    public int? NewLevel { get; init; }
    public Augment? NewAugment { get; init; }

    /// <summary>
    /// 计算两个状态之间的差异
    /// </summary>
    public static GameStateDiff Compute(GameState? oldState, GameState newState)
    {
        if (oldState is null)
            return new GameStateDiff
            {
                HasBoardChanged = true,
                HasBenchChanged = true,
                HasShopChanged = true,
                HasGoldChanged = true,
                HasHealthChanged = true,
                HasLevelChanged = true,
                HasRoundChanged = true,
                NewBoard = newState.ActivePlayer.Board,
                NewBench = newState.ActivePlayer.Bench,
                NewShop = newState.ActivePlayer.Shop,
                NewGold = newState.ActivePlayer.CurrentGold,
                NewHealth = newState.ActivePlayer.Health,
                NewLevel = newState.ActivePlayer.Level
            };

        var old = oldState.ActivePlayer;
        var cur = newState.ActivePlayer;

        return new GameStateDiff
        {
            HasBoardChanged = !old.Board.SequenceEqual(cur.Board),
            HasBenchChanged = !old.Bench.SequenceEqual(cur.Bench),
            HasShopChanged = !old.Shop.SequenceEqual(cur.Shop),
            HasGoldChanged = old.CurrentGold != cur.CurrentGold,
            HasHealthChanged = Math.Abs(old.Health - cur.Health) > 0.01,
            HasLevelChanged = old.Level != cur.Level,
            HasRoundChanged = oldState.GameInfo.Round != newState.GameInfo.Round,
            HasAugmentPicked = newState.Augments.Count > oldState.Augments.Count,
            NewBoard = cur.Board,
            NewBench = cur.Bench,
            NewShop = cur.Shop,
            OldGold = old.CurrentGold,
            NewGold = cur.CurrentGold,
            OldHealth = old.Health,
            NewHealth = cur.Health,
            OldLevel = old.Level,
            NewLevel = cur.Level,
            NewAugment = newState.Augments.Count > oldState.Augments.Count
                ? newState.Augments[^1] : null
        };
    }
}
```

---

### 5.2 数据获取抽象

#### 5.2.1 IGameDataSource — 数据源基础接口

```csharp
// src/TFTAssistant.Core/Abstractions/IGameDataSource.cs
namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 数据源基础接口 —— 对应 HDT 的 LogFileWatcher 基类
/// </summary>
public interface IGameDataSource
{
    Task StartAsync(CancellationToken ct);
    Task StopAsync();
    bool IsConnected { get; }
    DataSourceType Type { get; }
}

public enum DataSourceType
{
    LiveClientApi,
    OverwolfEvents,
    Mock
}
```

#### 5.2.2 IGameDataProvider — 实时游戏数据

```csharp
// src/TFTAssistant.Core/Abstractions/IGameDataProvider.cs
namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 实时游戏数据提供者 —— 对应 HDT 的 PowerHandler 输出
/// </summary>
public interface IGameDataProvider : IGameDataSource
{
    /// <summary>获取完整游戏状态</summary>
    Task<GameState?> GetFullStateAsync();

    /// <summary>状态变更事件（对应 HDT 的 TagChange 事件流）</summary>
    event EventHandler<GameStateDiff>? StateChanged;
}
```

#### 5.2.3 IStaticDataProvider — 静态数据

```csharp
// src/TFTAssistant.Core/Abstractions/IStaticDataProvider.cs
namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 静态数据提供者 —— 对应 HDT 的 HearthDb 卡牌数据库
/// </summary>
public interface IStaticDataProvider
{
    Task<IReadOnlyList<Champion>> GetChampionsAsync(string set);
    Task<IReadOnlyList<Item>> GetItemsAsync(string set);
    Task<IReadOnlyList<Trait>> GetTraitsAsync(string set);
    Task<IReadOnlyList<AugmentData>> GetAugmentsAsync(string set);
    Task<IReadOnlyList<MetaComp>> GetMetaCompsAsync(string set);
    Task<string> GetLatestSetVersionAsync();
}
```

#### 5.2.4 IMatchHistoryProvider — 赛后数据

```csharp
// src/TFTAssistant.Core/Abstractions/IMatchHistoryProvider.cs
namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 赛后对局数据提供者 —— HDT 没有的，TFT 新增
/// </summary>
public interface IMatchHistoryProvider
{
    Task<IReadOnlyList<MatchRecord>> GetRecentMatchesAsync(
        string puuid, int count = 20);
    Task<MatchDetail?> GetMatchDetailAsync(string matchId);
    Task<string?> GetPuuidByRiotIdAsync(string gameName, string tagLine);
}
```

---

### 5.3 推荐引擎

#### 5.3.1 推荐模型定义

```csharp
// src/TFTAssistant.Core/Models/Recommendation/RecommendationSet.cs
namespace TFTAssistant.Core.Models.Recommendation;

/// <summary>
/// 推荐结果集合 —— 一次推荐调用返回所有建议
/// </summary>
public sealed class RecommendationSet
{
    public required List<CompSuggestion> CompSuggestions { get; init; }
    public required List<ItemSuggestion> ItemSuggestions { get; init; }
    public required EconomyHint? EconomyHint { get; init; }
    public required List<AugmentRating> AugmentRatings { get; init; }
}

// src/TFTAssistant.Core/Models/Recommendation/CompSuggestion.cs
public sealed class CompSuggestion
{
    public required MetaComp Comp { get; init; }
    public required double Score { get; init; } // 0-100
    public required List<string> OwnedUnits { get; init; }
    public required List<string> MissingUnits { get; init; }
    public required double TraitCoverage { get; init; } // 0-1
}

// src/TFTAssistant.Core/Models/Recommendation/ItemSuggestion.cs
public sealed class ItemSuggestion
{
    public required string TargetChampion { get; init; }
    public required Item Item { get; init; }
    public required int Priority { get; init; } // 1-5，1 最高
    public required List<string> AvailableComponents { get; init; }
}

// src/TFTAssistant.Core/Models/Recommendation/EconomyHint.cs
public sealed class EconomyHint
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required HintUrgency Urgency { get; init; }
}

public enum HintUrgency
{
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

// src/TFTAssistant.Core/Models/Recommendation/AugmentRating.cs
public sealed class AugmentRating
{
    public required string AugmentId { get; init; }
    public required string AugmentName { get; init; }
    public required double Score { get; init; } // 0-100
    public required string Reason { get; init; }
}
```

#### 5.3.2 推荐引擎接口

```csharp
// src/TFTAssistant.Core/Abstractions/IRecommendationEngine.cs
namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 推荐引擎入口 —— HDT 没有的模块，TFT 特有
/// </summary>
public interface IRecommendationEngine
{
    Task<RecommendationSet> GetRecommendationsAsync(GameState state);
}

// src/TFTAssistant.Core/Abstractions/ICompMatcher.cs
public interface ICompMatcher
{
    Task<List<CompSuggestion>> MatchAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<BenchUnit> bench,
        IReadOnlyList<Augment> augments);
}

// src/TFTAssistant.Core/Abstractions/IItemAdvisor.cs
public interface IItemAdvisor
{
    Task<List<ItemSuggestion>> AdviseAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<ItemComponent> availableComponents);
}

// src/TFTAssistant.Core/Abstractions/IEconomyAdvisor.cs
public interface IEconomyAdvisor
{
    EconomyHint Advise(int gold, int level, int health,
                       bool winStreak, bool loseStreak, int stage);
}

// src/TFTAssistant.Core/Abstractions/IAugmentAdvisor.cs
public interface IAugmentAdvisor
{
    Task<List<AugmentRating>> RateAsync(
        IReadOnlyList<Augment> options,
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<Augment> existingAugments);
}
```

---

### 5.4 事件总线

```csharp
// src/TFTAssistant.Core/Abstractions/IEventBus.cs
namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 事件总线 —— 模块间解耦通信
/// 对应 HDT 中各 Handler 之间通过事件解耦的模式
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent e) where TEvent : IGameEvent;
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IGameEvent;
    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IGameEvent;
}

// src/TFTAssistant.Core/Models/Events/IGameEvent.cs
public interface IGameEvent { }

// src/TFTAssistant.Core/Models/Events/GameStarted.cs
public record GameStarted(string MatchId, string SetVersion) : IGameEvent;

// src/TFTAssistant.Core/Models/Events/GameEnded.cs
public record GameEnded(string MatchId, int Placement) : IGameEvent;

// src/TFTAssistant.Core/Models/Events/BoardChanged.cs
public record BoardChanged(IReadOnlyList<BoardUnit> NewBoard) : IGameEvent;

// src/TFTAssistant.Core/Models/Events/ShopRefreshed.cs
public record ShopRefreshed(IReadOnlyList<ShopUnit> NewShop) : IGameEvent;

// src/TFTAssistant.Core/Models/Events/AugmentPicked.cs
public record AugmentPicked(Augment Augment, int Stage) : IGameEvent;

// src/TFTAssistant.Core/Models/Events/PlayerLevelChanged.cs
public record PlayerLevelChanged(int OldLevel, int NewLevel) : IGameEvent;

// src/TFTAssistant.Core/Models/Events/HealthChanged.cs
public record HealthChanged(double OldHealth, double NewHealth) : IGameEvent;
```

#### 事件总线实现

```csharp
// src/TFTAssistant.Core/Events/InMemoryEventBus.cs
namespace TFTAssistant.Core.Events;

using TFTAssistant.Core.Abstractions;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    public Task PublishAsync<TEvent>(TEvent e) where TEvent : IGameEvent
    {
        var eventType = typeof(TEvent);
        if (!_handlers.TryGetValue(eventType, out var handlers))
            return Task.CompletedTask;

        var tasks = handlers
            .OfType<Func<TEvent, Task>>()
            .Select(h => h(e));

        return Task.WhenAll(tasks);
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IGameEvent
    {
        var eventType = typeof(TEvent);
        _handlers.AddOrUpdate(
            eventType,
            _ => new List<Delegate> { handler },
            (_, existing) =>
            {
                lock (existing) existing.Add(handler);
                return existing;
            });
    }

    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IGameEvent
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            lock (handlers) handlers.Remove(handler);
        }
    }
}
```

---

### 5.5 持久化

```csharp
// src/TFTAssistant.Core/Abstractions/IGameStateRepository.cs
namespace TFTAssistant.Core.Abstractions;

public interface IGameStateRepository
{
    Task SaveCurrentStateAsync(GameState state);
    Task<GameState?> LoadLastStateAsync();
    Task ClearCurrentStateAsync();
}

// src/TFTAssistant.Core/Abstractions/IMatchRepository.cs
public interface IMatchRepository
{
    Task SaveMatchAsync(MatchRecord match);
    Task<IReadOnlyList<MatchRecord>> GetMatchesAsync(
        DateTime? from = null, DateTime? to = null, int limit = 50);
    Task<MatchStats> GetAggregatedStatsAsync(TimeSpan period);
    Task UpdateCompStatsAsync(string compName, int placement);
}
```

---

## 6. 数据层实现

### 6.1 Live Client Data API 客户端

对应 HDT 的 `LogFileWatcher` —— 但实现更简单，只需 HTTP 请求。

```csharp
// src/TFTAssistant.Core/Data/LiveClientDataProvider.cs
namespace TFTAssistant.Core.Data;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

/// <summary>
/// Live Client Data API 客户端
/// 对应 HDT 的 LogFileWatcher —— 通过 HTTP 轮询替代文件监控
///
/// TFT 客户端在本地运行一个 HTTP 服务器 (localhost:2999)
/// 无需 API Key，直接请求即可获取完整游戏状态
/// </summary>
public sealed class LiveClientDataProvider : IGameDataProvider
{
    private const string BASE_URL = "https://127.0.0.1:2999/liveclientdata";
    private readonly HttpClient _http;
    private readonly ILogger<LiveClientDataProvider> _logger;
    private Timer? _pollTimer;
    private GameState? _lastState;
    private readonly object _lock = new();

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    public bool IsConnected { get; private set; }
    public DataSourceType Type => DataSourceType.LiveClientApi;

    public event EventHandler<GameStateDiff>? StateChanged;

    public LiveClientDataProvider(
        HttpClient http,
        ILogger<LiveClientDataProvider> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Live Client Data Provider 启动");
        _pollTimer = new Timer(
            async _ => await PollAsync(),
            null,
            TimeSpan.Zero,
            PollInterval);
    }

    public Task StopAsync()
    {
        _pollTimer?.Dispose();
        _pollTimer = null;
        IsConnected = false;
        _logger.LogInformation("Live Client Data Provider 停止");
        return Task.CompletedTask;
    }

    public Task<GameState?> GetFullStateAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_lastState);
        }
    }

    private async Task PollAsync()
    {
        try
        {
            var json = await _http.GetStringAsync(
                $"{BASE_URL}/allgamedata");

            var dto = JsonSerializer.Deserialize<LiveGameDataDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto is null) return;

            var newState = dto.ToGameState();

            lock (_lock)
            {
                if (!IsConnected)
                {
                    _logger.LogInformation("已连接到 Live Client API");
                    IsConnected = true;
                }

                var diff = GameStateDiff.Compute(_lastState, newState);

                if (diff.HasBoardChanged || diff.HasBenchChanged
                    || diff.HasShopChanged || diff.HasGoldChanged
                    || diff.HasHealthChanged || diff.HasLevelChanged
                    || diff.HasRoundChanged || diff.HasAugmentPicked)
                {
                    StateChanged?.Invoke(this, diff);
                }

                _lastState = newState;
            }
        }
        catch (HttpRequestException)
        {
            // 游戏未运行时 API 不可用，静默忽略
            if (IsConnected)
            {
                _logger.LogDebug("Live Client API 不可用（游戏可能未运行）");
                IsConnected = false;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Live Client API 返回数据解析失败");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Live Client Data Provider 轮询异常");
        }
    }
}
```

#### LiveGameDataDto — API 响应映射

```csharp
// src/TFTAssistant.Core/Data/LiveGameDataDto.cs
namespace TFTAssistant.Core.Data;

using TFTAssistant.Core.Models.Game;
using System.Text.Json.Serialization;

/// <summary>
/// Live Client API /allgamedata 端点的响应 DTO
/// 字段名与 API 返回的 JSON 结构对应
/// </summary>
public sealed class LiveGameDataDto
{
    [JsonPropertyName("allPlayers")]
    public List<LivePlayerDto> AllPlayers { get; set; } = new();

    [JsonPropertyName("activePlayer")]
    public LiveActivePlayerDto? ActivePlayer { get; set; }

    [JsonPropertyName("gameData")]
    public LiveGameInfoDto? GameData { get; set; }

    public GameState ToGameState()
    {
        var active = ActivePlayer!;
        var gameData = GameData!;

        return new GameState
        {
            ActivePlayer = new ActivePlayer
            {
                SummonerName = active.SummonerName,
                Level = active.Level,
                CurrentGold = active.CurrentGold,
                Health = active.Health,
                Experience = active.Experience,
                TotalGold = active.TotalGold,
                Placement = active.Placement,
                Board = active.Board.Select(b => new BoardUnit
                {
                    ChampionName = b.Character!.Name,
                    StarLevel = b.StarLevel,
                    Items = b.Items?.Select(i => i.Name ?? "").ToList()
                            ?? new List<string>(),
                    Row = b.TileY,
                    Column = b.TileX
                }).ToList(),
                Bench = active.Bench.Select((b, i) => new BenchUnit
                {
                    ChampionName = b.Character?.Name ?? "Unknown",
                    StarLevel = b.StarLevel,
                    Items = b.Items?.Select(i => i.Name ?? "").ToList()
                            ?? new List<string>(),
                    BenchSlot = i
                }).ToList(),
                Shop = active.Shop.Select((s, i) => new ShopUnit
                {
                    ChampionName = s.Character?.Name ?? "Unknown",
                    Cost = s.Cost,
                    ShopSlot = i
                }).ToList()
            },
            AllPlayers = AllPlayers.Select(p => new PlayerSummary
            {
                SummonerName = p.SummonerName,
                Health = p.Health,
                Placement = p.Placement,
                TotalGold = p.TotalGold,
                Level = p.Level
            }).ToList(),
            GameInfo = new GameInfo
            {
                GameTime = gameData.GameTime,
                Round = gameData.Round,
                Stage = gameData.Stage,
                IsPvp = gameData.IsPvp,
                SetNumber = $"Set{gameData.SetNumber}"
            },
            Augments = new List<Augment>()
        };
    }
}

// 以下为嵌套 DTO，字段名与 API JSON 对应
public sealed class LivePlayerDto
{
    public string SummonerName { get; set; } = "";
    public double Health { get; set; }
    public int Placement { get; set; }
    public int TotalGold { get; set; }
    public int Level { get; set; }
}

public sealed class LiveActivePlayerDto : LivePlayerDto
{
    public int CurrentGold { get; set; }
    public int Experience { get; set; }
    public List<LiveUnitDto> Board { get; set; } = new();
    public List<LiveUnitDto> Bench { get; set; } = new();
    public List<LiveShopUnitDto> Shop { get; set; } = new();
}

public sealed class LiveUnitDto
{
    public LiveCharacterDto? Character { get; set; }
    public int StarLevel { get; set; }
    public List<LiveItemDto>? Items { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
}

public sealed class LiveCharacterDto
{
    public string? Name { get; set; }
}

public sealed class LiveItemDto
{
    public string? Name { get; set; }
}

public sealed class LiveShopUnitDto
{
    public LiveCharacterDto? Character { get; set; }
    public int Cost { get; set; }
}

public sealed class LiveGameInfoDto
{
    public double GameTime { get; set; }
    public int Round { get; set; }
    public int Stage { get; set; }
    public bool IsPvp { get; set; }
    public int SetNumber { get; set; }
}
```

---

### 6.2 Overwolf 事件适配器

```csharp
// src/TFTAssistant.Core/Data/OverwolfEventAdapter.cs
namespace TFTAssistant.Core.Data;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

/// <summary>
/// Overwolf 事件适配器
/// 对应 HDT 的 HearthMirror —— 作为 Live API 的补充数据源
///
/// Overwolf 通过 JS 端监听游戏事件，然后通过 Bridge 调用此类的公共方法
/// </summary>
public sealed class OverwolfEventAdapter : IGameDataProvider
{
    private readonly ILogger<OverwolfEventAdapter> _logger;
    private GameState? _lastState;
    private readonly object _lock = new();

    public bool IsConnected { get; private set; }
    public DataSourceType Type => DataSourceType.OverwolfEvents;
    public event EventHandler<GameStateDiff>? StateChanged;

    public OverwolfEventAdapter(ILogger<OverwolfEventAdapter> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Overwolf 事件适配器启动（等待 JS Bridge 调用）");
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    public Task<GameState?> GetFullStateAsync()
    {
        lock (_lock) { return Task.FromResult(_lastState); }
    }

    /// <summary>
    /// 由 JS Bridge 调用 —— 接收 Overwolf info_update 事件
    /// </summary>
    public void OnInfoUpdate(string jsonPayload)
    {
        try
        {
            var update = JsonSerializer.Deserialize<OWInfoUpdate>(jsonPayload,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (update?.Info is null) return;

            IsConnected = true;
            var newState = OWInfoUpdateMapper.ToGameState(update.Info);

            lock (_lock)
            {
                var diff = GameStateDiff.Compute(_lastState, newState);
                if (diff.HasBoardChanged || diff.HasBenchChanged
                    || diff.HasShopChanged || diff.HasGoldChanged
                    || diff.HasHealthChanged || diff.HasLevelChanged)
                {
                    StateChanged?.Invoke(this, diff);
                }
                _lastState = newState;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Overwolf info_update 失败");
        }
    }

    /// <summary>
    /// 由 JS Bridge 调用 —— 接收 Overwolf 游戏事件
    /// </summary>
    public void OnGameEvent(string jsonPayload)
    {
        try
        {
            var events = JsonSerializer.Deserialize<List<OWGameEvent>>(jsonPayload);
            if (events is null) return;

            foreach (var e in events)
            {
                _logger.LogDebug("收到 Overwolf 事件: {Name}", e.Name);
                // 事件处理逻辑可在此扩展
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Overwolf game_event 失败");
        }
    }
}

// Overwolf 事件 DTO
internal sealed class OWInfoUpdate
{
    public OWInfoData? Info { get; set; }
}

internal sealed class OWInfoData
{
    public OWLiveClientData? LiveClientData { get; set; }
}

internal sealed class OWLiveClientData
{
    public List<OWPlayer>? AllPlayers { get; set; }
    public OWActivePlayer? ActivePlayer { get; set; }
    public OWGameData? GameData { get; set; }
}

internal sealed class OWPlayer
{
    public string? SummonerName { get; set; }
    public double Health { get; set; }
    public int Placement { get; set; }
    public int TotalGold { get; set; }
    public int Level { get; set; }
}

internal sealed class OWActivePlayer : OWPlayer
{
    public int CurrentGold { get; set; }
    public int Experience { get; set; }
}

internal sealed class OWGameData
{
    public double GameTime { get; set; }
    public int Round { get; set; }
    public int Stage { get; set; }
    public bool IsPvp { get; set; }
}

internal sealed class OWGameEvent
{
    public string? Name { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Overwolf 数据到 GameState 的映射器
/// </summary>
internal static class OWInfoUpdateMapper
{
    public static GameState ToGameState(OWLiveClientData data)
    {
        // 映射逻辑与 LiveGameDataDto.ToGameState 类似
        // 此处省略重复代码，实际开发中可抽取共用映射逻辑
        throw new NotImplementedException("根据 Overwolf API 文档实现映射");
    }
}
```

---

### 6.3 Data Dragon 静态数据

```csharp
// src/TFTAssistant.Core/Data/DataDragonProvider.cs
namespace TFTAssistant.Core.Data;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Static;

/// <summary>
/// Riot Data Dragon 静态数据提供者
/// 对应 HDT 的 HearthDb 卡牌数据库
///
/// Data Dragon 是 Riot 的官方 CDN，提供英雄/装备/特质等静态数据
/// 无需 API Key，直接通过 HTTP 获取
/// </summary>
public sealed class DataDragonProvider : IStaticDataProvider
{
    private const string DRAGON_BASE =
        "https://ddragon.leagueoflegends.com/cdn";
    private const string VERSIONS_URL =
        "https://ddragon.leagueoflegends.com/api/versions.json";

    private readonly HttpClient _http;
    private readonly ILogger<DataDragonProvider> _logger;
    private readonly string _localDataDir;

    // 内存缓存
    private string? _cachedVersion;
    private Dictionary<string, List<Champion>> _championCache = new();
    private Dictionary<string, List<Item>> _itemCache = new();
    private Dictionary<string, List<Trait>> _traitCache = new();

    public DataDragonProvider(
        HttpClient http,
        ILogger<DataDragonProvider> logger,
        string localDataDir = "data/sets")
    {
        _http = http;
        _logger = logger;
        _localDataDir = localDataDir;
    }

    public async Task<string> GetLatestSetVersionAsync()
    {
        if (_cachedVersion is not null)
            return _cachedVersion;

        var versions = await _http.GetFromJsonAsync<List<string>>(VERSIONS_URL);
        _cachedVersion = versions?[0] ?? "15.1.1";
        return _cachedVersion;
    }

    public async Task<IReadOnlyList<Champion>> GetChampionsAsync(string set)
    {
        if (_championCache.TryGetValue(set, out var cached))
            return cached;

        // 优先从本地文件加载
        var localPath = Path.Combine(_localDataDir, set, "champions.json");
        if (File.Exists(localPath))
        {
            var json = await File.ReadAllTextAsync(localPath);
            var champions = JsonSerializer.Deserialize<List<Champion>>(json)
                            ?? new List<Champion>();
            _championCache[set] = champions;
            return champions;
        }

        // 从 Data Dragon 下载
        var version = await GetLatestSetVersionAsync();
        var url = $"{DRAGON_BASE}/{version}/data/en_US/tft-champion.json";

        try
        {
            var response = await _http.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<JsonElement>(response);
            var champions = ParseChampions(data);

            // 缓存到本地
            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
            var serialized = JsonSerializer.Serialize(champions,
                new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(localPath, serialized);

            _championCache[set] = champions;
            return champions;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "从 Data Dragon 获取英雄数据失败");
            return new List<Champion>();
        }
    }

    public async Task<IReadOnlyList<Item>> GetItemsAsync(string set)
    {
        if (_itemCache.TryGetValue(set, out var cached))
            return cached;

        var localPath = Path.Combine(_localDataDir, set, "items.json");
        if (File.Exists(localPath))
        {
            var json = await File.ReadAllTextAsync(localPath);
            var items = JsonSerializer.Deserialize<List<Item>>(json)
                        ?? new List<Item>();
            _itemCache[set] = items;
            return items;
        }

        var version = await GetLatestSetVersionAsync();
        var url = $"{DRAGON_BASE}/{version}/data/en_US/tft-item.json";

        try
        {
            var response = await _http.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<JsonElement>(response);
            var items = ParseItems(data);

            Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
            await File.WriteAllTextAsync(localPath,
                JsonSerializer.Serialize(items, new JsonSerializerOptions
                { WriteIndented = true }));

            _itemCache[set] = items;
            return items;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "从 Data Dragon 获取装备数据失败");
            return new List<Item>();
        }
    }

    public async Task<IReadOnlyList<Trait>> GetTraitsAsync(string set)
    {
        if (_traitCache.TryGetValue(set, out var cached))
            return cached;

        var localPath = Path.Combine(_localDataDir, set, "traits.json");
        if (File.Exists(localPath))
        {
            var json = await File.ReadAllTextAsync(localPath);
            var traits = JsonSerializer.Deserialize<List<Trait>>(json)
                         ?? new List<Trait>();
            _traitCache[set] = traits;
            return traits;
        }

        // Traits 数据通常在 champion.json 的 data 字段中嵌套
        // 此处简化处理，实际需解析嵌套结构
        return new List<Trait>();
    }

    public async Task<IReadOnlyList<AugmentData>> GetAugmentsAsync(string set)
    {
        var localPath = Path.Combine(_localDataDir, set, "augments.json");
        if (File.Exists(localPath))
        {
            var json = await File.ReadAllTextAsync(localPath);
            return JsonSerializer.Deserialize<List<AugmentData>>(json)
                   ?? new List<AugmentData>();
        }
        return new List<AugmentData>();
    }

    public async Task<IReadOnlyList<MetaComp>> GetMetaCompsAsync(string set)
    {
        // Meta 阵容数据来自社区维护的本地文件（非 Riot 官方）
        var localPath = Path.Combine(_localDataDir, set, "meta-comps.json");
        if (File.Exists(localPath))
        {
            var json = await File.ReadAllTextAsync(localPath);
            return JsonSerializer.Deserialize<List<MetaComp>>(json)
                   ?? new List<MetaComp>();
        }
        return new List<MetaComp>();
    }

    // JSON 解析辅助方法
    private static List<Champion> ParseChampions(JsonElement data)
    {
        var champions = new List<Champion>();
        if (data.TryGetProperty("data", out var dataObj))
        {
            foreach (var prop in dataObj.EnumerateObject())
            {
                var champ = prop.Value;
                champions.Add(new Champion
                {
                    Name = champ.GetProperty("name").GetString() ?? "",
                    ApiName = prop.Name,
                    Cost = champ.GetProperty("cost").GetInt32(),
                    Traits = champ.GetProperty("traits")
                             .EnumerateArray()
                             .Select(t => t.GetString() ?? "")
                             .ToList(),
                    AbilityName = champ.GetProperty("ability")
                                       .GetProperty("name").GetString() ?? ""
                });
            }
        }
        return champions;
    }

    private static List<Item> ParseItems(JsonElement data)
    {
        var items = new List<Item>();
        if (data.TryGetProperty("data", out var dataObj))
        {
            foreach (var prop in dataObj.EnumerateObject())
            {
                var item = prop.Value;
                items.Add(new Item
                {
                    Name = item.GetProperty("name").GetString() ?? "",
                    Id = prop.Name,
                    Description = item.GetProperty("description").GetString() ?? "",
                    IsComponent = item.TryGetProperty("from", out _)
                                  || item.TryGetProperty("component", out _),
                    Components = item.TryGetProperty("from", out var from)
                        ? from.EnumerateArray().Select(f => f.GetString() ?? "").ToList()
                        : new List<string>()
                });
            }
        }
        return items;
    }
}
```

---

### 6.4 Riot API 赛后数据

```csharp
// src/TFTAssistant.Core/Data/RiotApiMatchProvider.cs
namespace TFTAssistant.Core.Data;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Analytics;

/// <summary>
/// Riot API 赛后数据提供者
/// 需要 API Key，用于获取对局历史和详细数据
///
/// API 端点:
///   - TFT-MATCH-V1: 对局记录
///   - TFT-LEAGUE-V1: 排位数据
///   - riot-account-v1: 账号信息
/// </summary>
public sealed class RiotApiMatchProvider : IMatchHistoryProvider
{
    private const string API_BASE = "https://{region}.api.riotgames.com";
    private const string ACCOUNT_BASE = "https://{region}.api.riotgames.com";

    private readonly HttpClient _http;
    private readonly ILogger<RiotApiMatchProvider> _logger;
    private readonly string _apiKey;
    private readonly string _platform; // "na1", "kr", "euw1" 等
    private readonly string _region;   // "americas", "asia", "europe"

    // 速率限制：Personal Key 为 20次/1秒, 100次/2分钟
    private readonly SemaphoreSlim _rateLimit = new(20, 20);
    private DateTime _lastRequestTime = DateTime.MinValue;

    public RiotApiMatchProvider(
        HttpClient http,
        ILogger<RiotApiMatchProvider> logger,
        string apiKey,
        string platform = "na1",
        string region = "americas")
    {
        _http = http;
        _logger = logger;
        _apiKey = apiKey;
        _platform = platform;
        _region = region;
    }

    public async Task<string?> GetPuuidByRiotIdAsync(
        string gameName, string tagLine)
    {
        await EnforceRateLimit();

        var url = ACCOUNT_BASE
            .Replace("{region}", _region)
            + $"/riot/account/v1/accounts/by-riot-id/{gameName}/{tagLine}"
            + $"?api_key={_apiKey}";

        try
        {
            var json = await _http.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<JsonElement>(json);
            return data.GetProperty("puuid").GetString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 PUUID 失败: {GameName}#{TagLine}",
                gameName, tagLine);
            return null;
        }
    }

    public async Task<IReadOnlyList<MatchRecord>> GetRecentMatchesAsync(
        string puuid, int count = 20)
    {
        await EnforceRateLimit();

        var url = API_BASE
            .Replace("{region}", _region)
            + $"/tft/match/v1/matches/by-puuid/{puuid}/ids"
            + $"?count={count}&api_key={_apiKey}";

        try
        {
            var json = await _http.GetStringAsync(url);
            var matchIds = JsonSerializer.Deserialize<List<string>>(json)
                           ?? new List<string>();

            return matchIds.Select(id => new MatchRecord
            {
                MatchId = id,
                Puuid = puuid
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取对局列表失败");
            return new List<MatchRecord>();
        }
    }

    public async Task<MatchDetail?> GetMatchDetailAsync(string matchId)
    {
        await EnforceRateLimit();

        var url = API_BASE
            .Replace("{region}", _region)
            + $"/tft/match/v1/matches/{matchId}"
            + $"?api_key={_apiKey}";

        try
        {
            var json = await _http.GetStringAsync(url);
            return JsonSerializer.Deserialize<MatchDetail>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取对局详情失败: {MatchId}", matchId);
            return null;
        }
    }

    private async Task EnforceRateLimit()
    {
        await _rateLimit.WaitAsync();

        var elapsed = DateTime.UtcNow - _lastRequestTime;
        if (elapsed < TimeSpan.FromMilliseconds(50)) // 20 req/s
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50) - elapsed);
        }

        _lastRequestTime = DateTime.UtcNow;
        _rateLimit.Release();
    }
}
```

---

### 6.5 组合数据源

```csharp
// src/TFTAssistant.Core/Data/CompositeGameDataProvider.cs
namespace TFTAssistant.Core.Data;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

/// <summary>
/// 组合数据源 —— 对应 HDT 的 LogWatcherManager
/// 按优先级尝试多个数据源，自动切换
/// </summary>
public sealed class CompositeGameDataProvider : IGameDataProvider
{
    private readonly List<IGameDataProvider> _sources;
    private readonly ILogger<CompositeGameDataProvider> _logger;

    public bool IsConnected => _sources.Any(s => s.IsConnected);
    public DataSourceType Type => DataSourceType.LiveClientApi; // 主要类型

    public event EventHandler<GameStateDiff>? StateChanged;

    public CompositeGameDataProvider(
        LiveClientDataProvider liveApi,
        OverwolfEventAdapter overwolf,
        MockGameDataProvider mock,
        ILogger<CompositeGameDataProvider> logger)
    {
        // 按优先级排序
        _sources = new[] { liveApi, overwolf, mock }.ToList();
        _logger = logger;

        // 转发状态变更事件
        foreach (var source in _sources)
        {
            source.StateChanged += (s, diff) => StateChanged?.Invoke(s, diff);
        }
    }

    public async Task StartAsync(CancellationToken ct)
    {
        foreach (var source in _sources)
        {
            await source.StartAsync(ct);
        }
    }

    public async Task StopAsync()
    {
        foreach (var source in _sources)
        {
            await source.StopAsync();
        }
    }

    public Task<GameState?> GetFullStateAsync()
    {
        // 优先使用 Live API，不可用时回退
        foreach (var source in _sources)
        {
            if (source.IsConnected)
            {
                return source.GetFullStateAsync();
            }
        }
        return Task.FromResult<GameState?>(null);
    }
}
```

---

## 7. 推荐引擎实现

### 7.1 阵容匹配器

```csharp
// src/TFTAssistant.Core/Engine/CompMatcher.cs
namespace TFTAssistant.Core.Engine;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;
using TFTAssistant.Core.Models.Static;

/// <summary>
/// 阵容匹配引擎
/// 根据当前棋盘/备战席状态，从 Meta 阵容库中匹配最合适的阵容
/// </summary>
public sealed class CompMatcher : ICompMatcher
{
    private readonly IStaticDataProvider _staticData;
    private readonly ILogger<CompMatcher> _logger;

    public CompMatcher(
        IStaticDataProvider staticData,
        ILogger<CompMatcher> logger)
    {
        _staticData = staticData;
        _logger = logger;
    }

    public async Task<List<CompSuggestion>> MatchAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<BenchUnit> bench,
        IReadOnlyList<Augment> augments)
    {
        // 1. 提取当前特征
        var ownedChampions = board.Concat(bench)
            .Select(u => u.ChampionName)
            .Distinct()
            .ToHashSet();

        var activeTraits = CalculateActiveTraits(board);

        // 2. 加载 Meta 阵容库
        var allComps = await _staticData.GetMetaCompsAsync("set13");
        if (allComps.Count == 0)
        {
            _logger.LogWarning("Meta 阵容库为空");
            return new List<CompSuggestion>();
        }

        // 3. 评分排序
        var scored = allComps
            .Select(comp => ScoreComp(comp, ownedChampions, activeTraits, augments))
            .OrderByDescending(s => s.Score)
            .Take(3)
            .ToList();

        return scored;
    }

    private CompSuggestion ScoreComp(
        MetaComp comp,
        HashSet<string> owned,
        Dictionary<string, int> traits,
        IReadOnlyList<Augment> augments)
    {
        // 已拥有棋子匹配度 (权重 40%)
        double ownedScore = comp.Units.Count(u => owned.Contains(u.Name))
                            / (double)comp.Units.Count * 40;

        // 羁绊激活度 (权重 30%)
        double traitScore = comp.RequiredTraits.Count(t =>
            traits.GetValueOrDefault(t.TraitId, 0) >= t.MinUnits)
            / (double)Math.Max(1, comp.RequiredTraits.Count) * 30;

        // 强化符文契合度 (权重 20%)
        double augmentScore = comp.SynergyAugments.Count(a =>
            augments.Any(owned => owned.Id == a))
            / (double)Math.Max(1, comp.SynergyAugments.Count) * 20;

        // 版本强度 (权重 10%)
        double tierScore = comp.TierScore * 10;

        var totalScore = ownedScore + traitScore + augmentScore + tierScore;

        return new CompSuggestion
        {
            Comp = comp,
            Score = Math.Round(totalScore, 1),
            OwnedUnits = comp.Units
                .Where(u => owned.Contains(u.Name))
                .Select(u => u.Name)
                .ToList(),
            MissingUnits = comp.Units
                .Where(u => !owned.Contains(u.Name))
                .Select(u => u.Name)
                .ToList(),
            TraitCoverage = Math.Round(traitScore / 30, 2)
        };
    }

    private Dictionary<string, int> CalculateActiveTraits(
        IReadOnlyList<BoardUnit> board)
    {
        // 需要根据英雄的特质列表计算当前激活的羁绊数量
        // 此处简化处理，实际需查询 Champion → Trait 映射
        return new Dictionary<string, int>();
    }
}
```

---

### 7.2 装备建议器

```csharp
// src/TFTAssistant.Core/Engine/ItemAdvisor.cs
namespace TFTAssistant.Core.Engine;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;
using TFTAssistant.Core.Models.Static;

/// <summary>
/// 装备建议引擎
/// 根据棋盘上的单位和可用装备组件，推荐最佳装备分配方案
/// </summary>
public sealed class ItemAdvisor : IItemAdvisor
{
    private readonly IStaticDataProvider _staticData;
    private readonly ILogger<ItemAdvisor> _logger;

    public ItemAdvisor(
        IStaticDataProvider staticData,
        ILogger<ItemAdvisor> logger)
    {
        _staticData = staticData;
        _logger = logger;
    }

    public async Task<List<ItemSuggestion>> AdviseAsync(
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<ItemComponent> availableComponents)
    {
        var suggestions = new List<ItemSuggestion>();
        var usedComponents = new HashSet<string>();

        // 按优先级处理：主 C（3星） > 副 C（2星） > 其他
        var sortedUnits = board
            .Where(u => u.StarLevel >= 2)
            .OrderByDescending(u => u.StarLevel)
            .ThenByDescending(u => u.Items.Count)
            .ToList();

        foreach (var unit in sortedUnits)
        {
            var bestItems = await GetBestItemsFor(unit.ChampionName);

            foreach (var item in bestItems)
            {
                // 检查组件是否可用且未被分配
                var neededComponents = item.Components
                    .Where(c => availableComponents.Any(ac => ac.Id == c)
                             && !usedComponents.Contains(c))
                    .ToList();

                if (neededComponents.Count == item.Components.Count)
                {
                    suggestions.Add(new ItemSuggestion
                    {
                        TargetChampion = unit.ChampionName,
                        Item = item,
                        Priority = item.PriorityFor(unit.ChampionName),
                        AvailableComponents = neededComponents
                    });

                    // 标记组件为已使用
                    foreach (var c in neededComponents)
                        usedComponents.Add(c);
                }
            }
        }

        return suggestions
            .OrderByDescending(s => s.Priority)
            .Take(5)
            .ToList();
    }

    private async Task<List<Item>> GetBestItemsFor(string championName)
    {
        // 从装备推荐规则表查询
        var builds = ItemBuildDatabase.Get(championName);
        if (builds.Count > 0)
            return builds;

        // 回退：从静态数据获取所有成品装备
        var allItems = await _staticData.GetItemsAsync("set13");
        return allItems.Where(i => !i.IsComponent).ToList();
    }
}
```

#### 装备推荐规则表

```csharp
// src/TFTAssistant.Core/Engine/ItemBuildDatabase.cs
namespace TFTAssistant.Core.Engine;

using TFTAssistant.Core.Models.Static;
using System.Collections.Frozen;

/// <summary>
/// 装备推荐规则表（静态数据，来自社区经验）
/// 每个英雄对应其最佳装备列表
/// </summary>
public static class ItemBuildDatabase
{
    private static readonly FrozenDictionary<string, List<Item>> _builds =
        new Dictionary<string, List<Item>>
        {
            // 示例数据，需根据当前赛季更新
            ["Ahri"] = new List<Item>
            {
                new() { Name = "Shojin", Id = "Shojin", Components = new() { "Tear", "Rod" }, Priority = 1 },
                new() { Name = "Blue Buff", Id = "BlueBuff", Components = new() { "Tear", "Rod" }, Priority = 2 },
                new() { Name = "Jeweled Gauntlet", Id = "JeweledGauntlet", Components = new() { "Glove", "Rod" }, Priority = 3 }
            }
            // ... 更多英雄的装备推荐
        }.ToFrozenDictionary();

    public static List<Item> Get(string championName)
    {
        return _builds.GetValueOrDefault(championName, new List<Item>());
    }

    public static void Update(string championName, List<Item> builds)
    {
        // 支持运行时更新（从配置文件加载）
        // _builds[championName] = builds;
    }
}
```

---

### 7.3 经济顾问

```csharp
// src/TFTAssistant.Core/Engine/EconomyAdvisor.cs
namespace TFTAssistant.Core.Engine;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Recommendation;

/// <summary>
/// 经济策略顾问
/// ⚠️ 所有建议都是基于通用规则的静态提示，不是根据实时局面动态生成的
/// 这确保了符合 Riot 第三方工具政策
/// </summary>
public sealed class EconomyAdvisor : IEconomyAdvisor
{
    public EconomyHint Advise(
        int gold, int level, int health,
        bool winStreak, bool loseStreak, int stage)
    {
        return (gold, health, stage) switch
        {
            // 低血量紧急情况
            (>= 0, <= 20, _) => new EconomyHint
            {
                Title = "血量危险",
                Description = "血量过低，考虑刷新寻找关键棋子保血",
                Urgency = HintUrgency.Critical
            },

            // 连胜/连败利息已满
            (>= 50, _, _) => new EconomyHint
            {
                Title = "利息已满",
                Description = $"当前 {gold} 金，可考虑升级或刷新",
                Urgency = HintUrgency.Low
            },

            // 前期存钱
            (< 50, > 50, <= 3) => new EconomyHint
            {
                Title = "前期运营",
                Description = "尽量保持 50 金吃利息",
                Urgency = HintUrgency.Medium
            },

            // 4阶段升级提示
            (_, _, 4) when level < 7 => new EconomyHint
            {
                Title = "4阶段升级",
                Description = "考虑在 4-1 或 4-2 升到 7/8 级",
                Urgency = HintUrgency.Medium
            },

            // 连胜奖励
            (_, _, _) when winStreak && gold >= 30 => new EconomyHint
            {
                Title = "连胜中",
                Description = "连胜提供额外收入，可适当刷新维持",
                Urgency = HintUrgency.Low
            },

            // 连败转搜
            (_, _, _) when loseStreak && stage >= 3 => new EconomyHint
            {
                Title = "连败转搜",
                Description = "连败补偿已累积，考虑开始搜牌转型",
                Urgency = HintUrgency.Medium
            },

            _ => new EconomyHint
            {
                Title = "正常运营",
                Description = "维持当前策略",
                Urgency = HintUrgency.None
            }
        };
    }
}
```

---

### 7.4 强化符文评分

```csharp
// src/TFTAssistant.Core/Engine/AugmentAdvisor.cs
namespace TFTAssistant.Core.Engine;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

/// <summary>
/// 强化符文评分引擎
/// 基于静态规则对可选强化符文进行评分
/// ⚠️ 不显示胜率数据（Riot 明确禁止）
/// </summary>
public sealed class AugmentAdvisor : IAugmentAdvisor
{
    private readonly IStaticDataProvider _staticData;
    private readonly ILogger<AugmentAdvisor> _logger;

    public AugmentAdvisor(
        IStaticDataProvider staticData,
        ILogger<AugmentAdvisor> logger)
    {
        _staticData = staticData;
        _logger = logger;
    }

    public async Task<List<AugmentRating>> RateAsync(
        IReadOnlyList<Augment> options,
        IReadOnlyList<BoardUnit> board,
        IReadOnlyList<Augment> existingAugments)
    {
        var ratings = new List<AugmentRating>();
        var boardChampions = board.Select(u => u.ChampionName).ToHashSet();

        foreach (var option in options)
        {
            double score = 50; // 基础分
            var reasons = new List<string>();

            // 规则1: 与当前棋盘英雄的契合度
            if (IsSynergyWithBoard(option, boardChampions))
            {
                score += 20;
                reasons.Add("与当前阵容契合");
            }

            // 规则2: 与已选符文的协同
            if (IsSynergyWithExisting(option, existingAugments))
            {
                score += 15;
                reasons.Add("与已选符文协同");
            }

            // 规则3: 通用强度评级（来自静态数据）
            var augmentData = await GetAugmentData(option.Id);
            if (augmentData is not null)
            {
                score += augmentData.BaseRating * 0.15;
                reasons.Add(augmentData.BaseRating > 7
                    ? "通用强度高" : "通用强度中等");
            }

            score = Math.Clamp(score, 0, 100);

            ratings.Add(new AugmentRating
            {
                AugmentId = option.Id,
                AugmentName = option.Name,
                Score = Math.Round(score, 1),
                Reason = string.Join("；", reasons)
            });
        }

        return ratings.OrderByDescending(r => r.Score).ToList();
    }

    private bool IsSynergyWithBoard(Augment augment, HashSet<string> boardChampions)
    {
        // 检查符文是否与棋盘上的英雄/羁绊相关
        // 实际实现需查询符文与英雄的关联表
        return false;
    }

    private bool IsSynergyWithExisting(Augment augment, IReadOnlyList<Augment> existing)
    {
        // 检查是否与已选符文形成协同
        return false;
    }

    private async Task<AugmentData?> GetAugmentData(string id)
    {
        var augments = await _staticData.GetAugmentsAsync("set13");
        return augments.FirstOrDefault(a => a.Id == id);
    }
}
```

---

### 7.5 推荐引擎总入口

```csharp
// src/TFTAssistant.Core/Engine/RecommendationEngine.cs
namespace TFTAssistant.Core.Engine;

using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;
using TFTAssistant.Core.Models.Recommendation;

/// <summary>
/// 推荐引擎总入口
/// 协调各子引擎，生成完整的推荐结果集
/// </summary>
public sealed class RecommendationEngine : IRecommendationEngine
{
    private readonly ICompMatcher _compMatcher;
    private readonly IItemAdvisor _itemAdvisor;
    private readonly IEconomyAdvisor _economyAdvisor;
    private readonly IAugmentAdvisor _augmentAdvisor;
    private readonly ILogger<RecommendationEngine> _logger;

    public RecommendationEngine(
        ICompMatcher compMatcher,
        IItemAdvisor itemAdvisor,
        IEconomyAdvisor economyAdvisor,
        IAugmentAdvisor augmentAdvisor,
        ILogger<RecommendationEngine> logger)
    {
        _compMatcher = compMatcher;
        _itemAdvisor = itemAdvisor;
        _economyAdvisor = economyAdvisor;
        _augmentAdvisor = augmentAdvisor;
        _logger = logger;
    }

    public async Task<RecommendationSet> GetRecommendationsAsync(GameState state)
    {
        try
        {
            // 并行调用各子引擎
            var compTask = _compMatcher.MatchAsync(
                state.ActivePlayer.Board,
                state.ActivePlayer.Bench,
                state.Augments);

            var itemTask = _itemAdvisor.AdviseAsync(
                state.ActivePlayer.Board,
                GetAvailableComponents(state));

            // 经济建议是同步的纯规则计算
            var econHint = _economyAdvisor.Advise(
                state.ActivePlayer.CurrentGold,
                state.ActivePlayer.Level,
                (int)state.ActivePlayer.Health,
                state.ActivePlayer.TotalGold > state.ActivePlayer.CurrentGold + 5,
                false, // loseStreak 需从历史推断
                state.GameInfo.Stage);

            await Task.WhenAll(compTask, itemTask);

            return new RecommendationSet
            {
                CompSuggestions = await compTask,
                ItemSuggestions = await itemTask,
                EconomyHint = econHint,
                AugmentRatings = new List<AugmentRating>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成推荐失败");
            return new RecommendationSet
            {
                CompSuggestions = new List<CompSuggestion>(),
                ItemSuggestions = new List<ItemSuggestion>(),
                EconomyHint = null,
                AugmentRatings = new List<AugmentRating>()
            };
        }
    }

    private static List<ItemComponent> GetAvailableComponents(GameState state)
    {
        // 从备战席和棋盘上未合成的装备中提取组件
        // 实际实现需解析装备状态
        return new List<ItemComponent>();
    }
}
```

---

## 8. Overwolf 集成层

### 8.1 应用清单 manifest.json

```json
{
  "manifest_version": 1,
  "app_name": "TFT Assistant",
  "type": "Extension",
  "description": "个人云顶之弈辅助工具",
  "version": "0.1.0",
  "author": "Your Name",
  "permissions": [
    "GameInfo",
    "Hotkeys",
    "Desktop",
    "Extensions"
  ],
  "game_events": [5426],
  "features": [
    "live_client_data",
    "match_info",
    "game_info",
    "match_state"
  ],
  "icon": "src/ui/icon.png",
  "icon_gray": "src/ui/icon_gray.png",
  "windows": [
    {
      "name": "overlay",
      "window_type": "Overwolf",
      "title": "TFT Assistant Overlay",
      "transparent": true,
      "resizable": false,
      "size": {
        "width": 320,
        "height": 600
      },
      "min_size": {
        "width": 280,
        "height": 400
      },
      "grab_focus_on_click": false,
      "in_game_only": true,
      "show_in_taskbar": false,
      "file": "src/ui/overlay/index.html"
    },
    {
      "name": "desktop",
      "window_type": "Desktop",
      "title": "TFT Assistant",
      "transparent": false,
      "resizable": true,
      "size": {
        "width": 900,
        "height": 700
      },
      "min_size": {
        "width": 700,
        "height": 500
      },
      "show_in_taskbar": true,
      "file": "src/ui/desktop/index.html"
    },
    {
      "name": "background",
      "window_type": "Background",
      "transparent": false,
      "grab_focus_on_click": false,
      "show_in_taskbar": false,
      "file": "src/ui/background/index.html"
    }
  ],
  "hotkeys": {
    "toggle_overlay": {
      "title": "显示/隐藏覆盖层",
      "action": "toggle_overlay",
      "default_keys": [
        "Ctrl+Shift+T"
      ]
    }
  },
  "launch_events": [
    {
      "event": "GameLaunched",
      "game_id": 5426,
      "wait_for_game_info": true,
      "min_game_version": "",
      "max_game_version": ""
    }
  ],
  "block_window_list": [],
  "externally_connectable": {
    "matches": ["https://localhost:*/*"]
  },
  "developer": {
    "install_source": "dev"
  }
}
```

### 8.2 C# ↔ JS 桥接

#### C# 端 — OWBridge

```csharp
// src/TFTAssistant.Overwolf/Bridge/OWBridge.cs
namespace TFTAssistant.Overwolf.Bridge;

using System.Text.Json;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Models.Game;

/// <summary>
/// C# ↔ JavaScript 通信桥
/// Overwolf 窗口本质是 Chromium 浏览器
/// 通过 window.cefSharp.postMessage / overwolf.web.sendMessage 通信
/// </summary>
public sealed class OWBridge : IDisposable
{
    private readonly IGameDataProvider _dataProvider;
    private readonly IRecommendationEngine _recommendationEngine;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OWBridge> _logger;
    private readonly CancellationTokenSource _cts = new();

    public OWBridge(
        IGameDataProvider dataProvider,
        IRecommendationEngine recommendationEngine,
        IEventBus eventBus,
        ILogger<OWBridge> logger)
    {
        _dataProvider = dataProvider;
        _recommendationEngine = recommendationEngine;
        _eventBus = eventBus;
        _logger = logger;

        // 订阅游戏状态变更，自动推送给 UI
        _dataProvider.StateChanged += OnStateChanged;
    }

    /// <summary>
    /// 由 JS 端调用 —— 接收来自 Overwolf 的事件数据
    /// </summary>
    public async Task OnMessageReceivedAsync(string jsonMessage)
    {
        try
        {
            var msg = JsonSerializer.Deserialize<OWMessage>(jsonMessage,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (msg is null) return;

            switch (msg.Type)
            {
                case "overwolf_info_update":
                    // 转发给 OverwolfEventAdapter
                    if (_dataProvider is Data.OverwolfEventAdapter adapter)
                        adapter.OnInfoUpdate(msg.Payload?.ToString() ?? "");
                    break;

                case "overwolf_game_event":
                    if (_dataProvider is Data.OverwolfEventAdapter adapter2)
                        adapter2.OnGameEvent(msg.Payload?.ToString() ?? "");
                    break;

                case "ui_ready":
                    // UI 已就绪，发送当前状态
                    await SendCurrentStateAsync();
                    break;

                case "request_recommendations":
                    await SendRecommendationsAsync();
                    break;

                case "toggle_overlay":
                    // 切换覆盖层显示
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 Bridge 消息失败");
        }
    }

    private async void OnStateChanged(object? sender, GameStateDiff diff)
    {
        try
        {
            var json = JsonSerializer.Serialize(new OWMessage
            {
                Type = "state_changed",
                Payload = diff
            });
            await SendToUIAsync("state_changed", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送状态变更到 UI 失败");
        }
    }

    private async Task SendCurrentStateAsync()
    {
        var state = await _dataProvider.GetFullStateAsync();
        if (state is not null)
        {
            var json = JsonSerializer.Serialize(new OWMessage
            {
                Type = "full_state",
                Payload = state
            });
            await SendToUIAsync("full_state", json);
        }
    }

    private async Task SendRecommendationsAsync()
    {
        var state = await _dataProvider.GetFullStateAsync();
        if (state is null) return;

        var recommendations = await _recommendationEngine
            .GetRecommendationsAsync(state);

        var json = JsonSerializer.Serialize(new OWMessage
        {
            Type = "recommendations",
            Payload = recommendations
        });
        await SendToUIAsync("recommendations", json);
    }

    private async Task SendToUIAsync(string type, string json)
    {
        // 通过 Overwolf API 发送消息到所有窗口
        // 实际实现使用 overwolf.web.sendMessage
        _logger.LogDebug("发送到 UI: {Type}", type);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _dataProvider.StateChanged -= OnStateChanged;
    }
}

// Bridge 消息格式
internal sealed class OWMessage
{
    public string? Type { get; set; }
    public object? Payload { get; set; }
}
```

#### JS 端 — bridge-client.js

```javascript
// src/ui/overlay/js/bridge-client.js

/**
 * C# ↔ JS 通信桥（JS 端）
 * 通过 overwolf.web.sendMessage / onMessageReceived 与 C# 后端通信
 */

const BridgeClient = {
    _handlers: {},
    _isReady: false,

    /**
     * 初始化桥接
     */
    init() {
        // 监听来自 C# 的消息
        overwolf.web.onMessageReceived.addListener((message) => {
            try {
                const data = typeof message === 'string'
                    ? JSON.parse(message) : message;

                const handlers = this._handlers[data.type] || [];
                handlers.forEach(handler => handler(data.payload));
            } catch (e) {
                console.error('Bridge message parse error:', e);
            }
        });

        // 通知 C# 端 UI 已就绪
        this.send('ui_ready', {});
        this._isReady = true;
        console.log('[Bridge] Initialized');
    },

    /**
     * 发送消息到 C# 后端
     */
    send(type, payload) {
        const message = JSON.stringify({ type, payload });
        overwolf.web.sendMessage(message);
    },

    /**
     * 注册消息处理器
     */
    on(type, handler) {
        if (!this._handlers[type]) {
            this._handlers[type] = [];
        }
        this._handlers[type].push(handler);
    },

    /**
     * 移除消息处理器
     */
    off(type, handler) {
        if (!this._handlers[type]) return;
        this._handlers[type] = this._handlers[type].filter(h => h !== handler);
    },

    /**
     * 请求推荐数据
     */
    requestRecommendations() {
        this.send('request_recommendations', {});
    },

    /**
     * 切换覆盖层
     */
    toggleOverlay() {
        this.send('toggle_overlay', {});
    }
};

// 导出
window.BridgeClient = BridgeClient;
```

---

### 8.3 窗口管理

```csharp
// src/TFTAssistant.Overwolf/Windows/OWWindowManager.cs
namespace TFTAssistant.Overwolf.Windows;

/// <summary>
/// Overwolf 窗口生命周期管理
/// </summary>
public sealed class OWWindowManager
{
    private readonly ILogger<OWWindowManager> _logger;

    public OWWindowManager(ILogger<OWWindowManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 显示覆盖层窗口
    /// </summary>
    public async Task ShowOverlayAsync()
    {
        // 通过 Overwolf API 调用
        // overwolf.windows.obtainDeclaredWindow("overlay")
        // overwolf.windows.restore(windowId)
        _logger.LogDebug("显示覆盖层");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 隐藏覆盖层窗口
    /// </summary>
    public async Task HideOverlayAsync()
    {
        // overwolf.windows.minimize(windowId)
        _logger.LogDebug("隐藏覆盖层");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 切换覆盖层显示状态
    /// </summary>
    public async Task ToggleOverlayAsync()
    {
        // var window = await GetCurrentWindowAsync();
        // if (window.state === "minimized") restore else minimize
        await Task.CompletedTask;
    }
}
```

---

## 9. 前端 UI 设计

### 9.1 覆盖层 UI

#### index.html

```html
<!-- src/ui/overlay/index.html -->
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="UTF-8">
    <title>TFT Assistant Overlay</title>
    <link rel="stylesheet" href="css/reset.css">
    <link rel="stylesheet" href="css/variables.css">
    <link rel="stylesheet" href="css/overlay.css">
</head>
<body>
    <div id="app" class="overlay-container">
        <!-- 标题栏（可拖拽） -->
        <div class="overlay-header" ow-drag>
            <span class="overlay-title">TFT Assistant</span>
            <button class="btn-minimize" id="btn-minimize">—</button>
        </div>

        <!-- Tab 切换 -->
        <div class="tab-bar">
            <button class="tab active" data-tab="board">棋盘</button>
            <button class="tab" data-tab="items">装备</button>
            <button class="tab" data-tab="comps">阵容</button>
            <button class="tab" data-tab="econ">经济</button>
        </div>

        <!-- Tab 内容 -->
        <div class="tab-content">
            <!-- 棋盘追踪面板 -->
            <div class="tab-panel active" id="panel-board"></div>

            <!-- 装备面板 -->
            <div class="tab-panel" id="panel-items"></div>

            <!-- 阵容建议面板 -->
            <div class="tab-panel" id="panel-comps"></div>

            <!-- 经济面板 -->
            <div class="tab-panel" id="panel-econ"></div>
        </div>

        <!-- 底部状态栏 -->
        <div class="overlay-footer">
            <span id="connection-status" class="status-dot disconnected"></span>
            <span id="round-info">--</span>
        </div>
    </div>

    <script src="js/bridge-client.js"></script>
    <script src="js/state-store.js"></script>
    <script src="js/components/board-panel.js"></script>
    <script src="js/components/item-panel.js"></script>
    <script src="js/components/comp-panel.js"></script>
    <script src="js/components/econ-panel.js"></script>
    <script src="js/app.js"></script>
</body>
</html>
```

#### overlay.css

```css
/* src/ui/overlay/css/overlay.css */

:root {
    --bg-primary: rgba(10, 15, 30, 0.85);
    --bg-secondary: rgba(20, 30, 50, 0.9);
    --bg-hover: rgba(40, 60, 100, 0.8);
    --text-primary: #e0e6f0;
    --text-secondary: #8899b0;
    --accent: #4fc3f7;
    --accent-dim: rgba(79, 195, 247, 0.3);
    --success: #66bb6a;
    --warning: #ffa726;
    --danger: #ef5350;
    --border: rgba(79, 195, 247, 0.2);
    --cost-1: #8899aa;
    --cost-2: #66bb6a;
    --cost-3: #4fc3f7;
    --cost-4: #ce93d8;
    --cost-5: #ffa726;
}

* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
    font-family: 'Segoe UI', 'Microsoft YaHei', sans-serif;
    color: var(--text-primary);
    font-size: 13px;
}

body {
    background: transparent;
    overflow: hidden;
    user-select: none;
}

.overlay-container {
    width: 320px;
    height: 600px;
    background: var(--bg-primary);
    border: 1px solid var(--border);
    border-radius: 8px;
    display: flex;
    flex-direction: column;
    backdrop-filter: blur(10px);
}

/* 标题栏 */
.overlay-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px 12px;
    background: var(--bg-secondary);
    border-bottom: 1px solid var(--border);
    border-radius: 8px 8px 0 0;
    cursor: move;
}

.overlay-title {
    font-size: 14px;
    font-weight: 600;
    color: var(--accent);
}

.btn-minimize {
    background: none;
    border: 1px solid var(--border);
    color: var(--text-secondary);
    width: 24px;
    height: 24px;
    border-radius: 4px;
    cursor: pointer;
    font-size: 14px;
}

.btn-minimize:hover {
    background: var(--bg-hover);
    color: var(--text-primary);
}

/* Tab 栏 */
.tab-bar {
    display: flex;
    border-bottom: 1px solid var(--border);
}

.tab {
    flex: 1;
    padding: 8px 4px;
    background: transparent;
    border: none;
    color: var(--text-secondary);
    cursor: pointer;
    font-size: 12px;
    transition: all 0.2s;
    border-bottom: 2px solid transparent;
}

.tab:hover {
    color: var(--text-primary);
    background: var(--bg-hover);
}

.tab.active {
    color: var(--accent);
    border-bottom-color: var(--accent);
}

/* Tab 内容 */
.tab-content {
    flex: 1;
    overflow-y: auto;
    padding: 8px;
}

.tab-panel {
    display: none;
}

.tab-panel.active {
    display: block;
}

/* 底部状态栏 */
.overlay-footer {
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 6px 12px;
    border-top: 1px solid var(--border);
    font-size: 11px;
    color: var(--text-secondary);
}

.status-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
}

.status-dot.connected {
    background: var(--success);
    box-shadow: 0 0 4px var(--success);
}

.status-dot.disconnected {
    background: var(--danger);
}

/* 棋子卡片 */
.unit-card {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 8px;
    margin-bottom: 4px;
    background: var(--bg-secondary);
    border-radius: 6px;
    border-left: 3px solid var(--cost-1);
}

.unit-card[data-cost="2"] { border-left-color: var(--cost-2); }
.unit-card[data-cost="3"] { border-left-color: var(--cost-3); }
.unit-card[data-cost="4"] { border-left-color: var(--cost-4); }
.unit-card[data-cost="5"] { border-left-color: var(--cost-5); }

.unit-name {
    font-weight: 600;
    font-size: 13px;
}

.unit-stars {
    color: var(--cost-5);
    font-size: 11px;
}

.unit-items {
    display: flex;
    gap: 2px;
    margin-left: auto;
}

.item-icon {
    width: 18px;
    height: 18px;
    background: var(--bg-hover);
    border-radius: 3px;
    border: 1px solid var(--border);
}

/* 阵容建议 */
.comp-card {
    padding: 8px;
    margin-bottom: 6px;
    background: var(--bg-secondary);
    border-radius: 6px;
    border: 1px solid var(--border);
}

.comp-card.top {
    border-color: var(--accent);
}

.comp-name {
    font-weight: 600;
    font-size: 13px;
    margin-bottom: 4px;
}

.comp-score {
    float: right;
    color: var(--accent);
    font-weight: 600;
}

.comp-units {
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
    margin-top: 4px;
}

.comp-unit {
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 11px;
    background: var(--bg-hover);
}

.comp-unit.owned {
    background: var(--accent-dim);
    color: var(--accent);
}

.comp-unit.missing {
    color: var(--text-secondary);
    opacity: 0.6;
}

/* 经济提示 */
.econ-hint {
    padding: 10px;
    border-radius: 6px;
    background: var(--bg-secondary);
    border-left: 3px solid var(--text-secondary);
}

.econ-hint.urgency-low { border-left-color: var(--success); }
.econ-hint.urgency-medium { border-left-color: var(--warning); }
.econ-hint.urgency-high { border-left-color: var(--danger); }
.econ-hint.urgency-critical {
    border-left-color: var(--danger);
    background: rgba(239, 83, 80, 0.15);
}

.econ-title {
    font-weight: 600;
    margin-bottom: 4px;
}

.econ-description {
    font-size: 12px;
    color: var(--text-secondary);
}

/* 滚动条 */
.tab-content::-webkit-scrollbar {
    width: 4px;
}

.tab-content::-webkit-scrollbar-track {
    background: transparent;
}

.tab-content::-webkit-scrollbar-thumb {
    background: var(--border);
    border-radius: 2px;
}
```

#### app.js — 前端入口

```javascript
// src/ui/overlay/js/app.js

(function() {
    'use strict';

    // 初始化
    document.addEventListener('DOMContentLoaded', () => {
        BridgeClient.init();
        initTabs();
        initMinimize();
        initMessageHandlers();
    });

    // Tab 切换
    function initTabs() {
        const tabs = document.querySelectorAll('.tab');
        tabs.forEach(tab => {
            tab.addEventListener('click', () => {
                tabs.forEach(t => t.classList.remove('active'));
                document.querySelectorAll('.tab-panel')
                    .forEach(p => p.classList.remove('active'));

                tab.classList.add('active');
                document.getElementById(`panel-${tab.dataset.tab}`)
                    .classList.add('active');
            });
        });
    }

    // 最小化按钮
    function initMinimize() {
        document.getElementById('btn-minimize')
            .addEventListener('click', () => {
                BridgeClient.toggleOverlay();
            });
    }

    // 注册消息处理器
    function initMessageHandlers() {
        // 接收完整状态
        BridgeClient.on('full_state', (state) => {
            StateStore.update(state);
            BoardPanel.render(state.activePlayer);
            updateConnectionStatus(true);
            updateRoundInfo(state.gameInfo);
        });

        // 接收状态变更
        BridgeClient.on('state_changed', (diff) => {
            StateStore.applyDiff(diff);
            BoardPanel.update(diff);
            updateRoundInfo(diff);
        });

        // 接收推荐结果
        BridgeClient.on('recommendations', (recs) => {
            CompPanel.render(recs.compSuggestions);
            ItemPanel.render(recs.itemSuggestions);
            EconPanel.render(recs.economyHint);
        });
    }

    function updateConnectionStatus(connected) {
        const dot = document.getElementById('connection-status');
        dot.className = `status-dot ${connected ? 'connected' : 'disconnected'}`;
    }

    function updateRoundInfo(info) {
        const el = document.getElementById('round-info');
        if (info && info.round) {
            el.textContent = `阶段 ${info.stage} 回合 ${info.round}`;
        }
    }
})();
```

---

### 9.2 桌面主窗口

桌面主窗口用于对局历史查看和数据统计，结构类似但更复杂。核心页面包括：

- **对局历史**：表格展示近期对局，点击查看详情
- **统计仪表盘**：胜率、前四率、吃鸡率图表
- **设置页面**：API Key 配置、显示偏好、快捷键设置

（具体 HTML/CSS/JS 实现与覆盖层类似，此处省略重复代码）

---

### 9.3 前端状态管理

```javascript
// src/ui/overlay/js/state-store.js

/**
 * 前端状态管理 —— 简单的响应式状态存储
 * 对应 HDT 的 GameV2 在 UI 层的镜像
 */
const StateStore = {
    _state: null,
    _listeners: [],

    update(fullState) {
        this._state = fullState;
        this._notify('update', fullState);
    },

    applyDiff(diff) {
        if (!this._state) return;

        if (diff.hasBoardChanged) {
            this._state.activePlayer.board = diff.newBoard;
        }
        if (diff.hasBenchChanged) {
            this._state.activePlayer.bench = diff.newBench;
        }
        if (diff.hasShopChanged) {
            this._state.activePlayer.shop = diff.newShop;
        }
        if (diff.hasGoldChanged) {
            this._state.activePlayer.currentGold = diff.newGold;
        }
        if (diff.hasHealthChanged) {
            this._state.activePlayer.health = diff.newHealth;
        }
        if (diff.hasLevelChanged) {
            this._state.activePlayer.level = diff.newLevel;
        }

        this._notify('diff', diff);
    },

    getState() {
        return this._state;
    },

    on(event, handler) {
        this._listeners.push({ event, handler });
    },

    _notify(event, data) {
        this._listeners
            .filter(l => l.event === event)
            .forEach(l => l.handler(data));
    }
};

window.StateStore = StateStore;
```

---

## 10. 数据库设计

```sql
-- DatabaseInitializer.cs 中执行

-- 对局记录
CREATE TABLE IF NOT EXISTS matches (
    id              TEXT PRIMARY KEY,       -- Riot matchId
    puuid           TEXT NOT NULL,
    set_version     TEXT NOT NULL,          -- "Set13"
    placement       INTEGER NOT NULL,       -- 1-8
    played_at       TEXT NOT NULL,          -- ISO 8601 UTC
    duration_seconds INTEGER,
    last_round      INTEGER,
    final_comp      TEXT,                   -- 最终阵容名
    augments        TEXT,                   -- JSON array
    created_at      TEXT DEFAULT (datetime('now'))
);

-- 对局回合快照
CREATE TABLE IF NOT EXISTS match_rounds (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    match_id        TEXT NOT NULL REFERENCES matches(id) ON DELETE CASCADE,
    round_number    INTEGER NOT NULL,
    level           INTEGER,
    gold            INTEGER,
    health          REAL,
    board_snapshot  TEXT,                   -- JSON: BoardUnit[]
    bench_snapshot  TEXT,                   -- JSON: BenchUnit[]
    created_at      TEXT DEFAULT (datetime('now')),
    UNIQUE(match_id, round_number)
);

-- 阵容统计（聚合表）
CREATE TABLE IF NOT EXISTS comp_stats (
    comp_name       TEXT PRIMARY KEY,
    set_version     TEXT NOT NULL,
    games_played    INTEGER DEFAULT 0,
    total_placement REAL DEFAULT 0.0,       -- 所有名次之和
    top4_count      INTEGER DEFAULT 0,       -- 前四次数
    win_count       INTEGER DEFAULT 0,       -- 吃鸡次数
    last_updated    TEXT DEFAULT (datetime('now'))
);

-- 用户设置
CREATE TABLE IF NOT EXISTS settings (
    key             TEXT PRIMARY KEY,
    value           TEXT NOT NULL,
    updated_at      TEXT DEFAULT (datetime('now'))
);

-- 索引
CREATE INDEX IF NOT EXISTS idx_matches_puuid ON matches(puuid);
CREATE INDEX IF NOT EXISTS idx_matches_played_at ON matches(played_at);
CREATE INDEX IF NOT EXISTS idx_matches_set ON matches(set_version);
CREATE INDEX IF NOT EXISTS idx_match_rounds_match ON match_rounds(match_id);
```

#### 数据库初始化实现

```csharp
// src/TFTAssistant.Core/Storage/DatabaseInitializer.cs
namespace TFTAssistant.Core.Storage;

using Microsoft.Data.Sqlite;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(string dbPath)
    {
        await using var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync();

        var commands = new[]
        {
            """
            CREATE TABLE IF NOT EXISTS matches (
                id TEXT PRIMARY KEY,
                puuid TEXT NOT NULL,
                set_version TEXT NOT NULL,
                placement INTEGER NOT NULL,
                played_at TEXT NOT NULL,
                duration_seconds INTEGER,
                last_round INTEGER,
                final_comp TEXT,
                augments TEXT,
                created_at TEXT DEFAULT (datetime('now'))
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS match_rounds (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                match_id TEXT NOT NULL REFERENCES matches(id) ON DELETE CASCADE,
                round_number INTEGER NOT NULL,
                level INTEGER,
                gold INTEGER,
                health REAL,
                board_snapshot TEXT,
                bench_snapshot TEXT,
                created_at TEXT DEFAULT (datetime('now')),
                UNIQUE(match_id, round_number)
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS comp_stats (
                comp_name TEXT PRIMARY KEY,
                set_version TEXT NOT NULL,
                games_played INTEGER DEFAULT 0,
                total_placement REAL DEFAULT 0.0,
                top4_count INTEGER DEFAULT 0,
                win_count INTEGER DEFAULT 0,
                last_updated TEXT DEFAULT (datetime('now'))
            )
            """,
            """
            CREATE TABLE IF NOT EXISTS settings (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL,
                updated_at TEXT DEFAULT (datetime('now'))
            )
            """,
            "CREATE INDEX IF NOT EXISTS idx_matches_puuid ON matches(puuid)",
            "CREATE INDEX IF NOT EXISTS idx_matches_played_at ON matches(played_at)",
            "CREATE INDEX IF NOT EXISTS idx_match_rounds_match ON match_rounds(match_id)"
        };

        foreach (var sql in commands)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
```

---

## 11. 依赖注入与启动流程

```csharp
// src/TFTAssistant.App/Bootstrapper.cs
namespace TFTAssistant.App;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Engine;
using TFTAssistant.Core.Events;
using TFTAssistant.Core.Storage;
using TFTAssistant.Overwolf.Bridge;
using TFTAssistant.Overwolf.Windows;

public static class Bootstrapper
{
    public static IServiceProvider Build()
    {
        // 配置 Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/tft-assistant-.log",
                rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();

        var services = new ServiceCollection();

        // === 日志 ===
        services.AddLogging(builder => builder.AddSerilog());

        // === 数据源（按优先级注册） ===
        services.AddHttpClient<LiveClientDataProvider>(client =>
        {
            // Live API 使用自签名证书
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        services.AddSingleton<OverwolfEventAdapter>();
        services.AddSingleton<MockGameDataProvider>();
        services.AddSingleton<CompositeGameDataProvider>();

        // === 静态数据 ===
        services.AddHttpClient<DataDragonProvider>();
        services.AddSingleton<IStaticDataProvider, DataDragonProvider>();

        // === 赛后数据（需要 API Key） ===
        // 从配置文件读取
        var apiKey = Environment.GetEnvironmentVariable("RIOT_API_KEY") ?? "";
        services.AddSingleton<IMatchHistoryProvider>(
            new RiotApiMatchProvider(
                new HttpClient(),
                LoggerFactory.Create(b => b.AddSerilog())
                    .CreateLogger<RiotApiMatchProvider>(),
                apiKey));

        // === 推荐引擎（插件化，可独立替换） ===
        services.AddSingleton<ICompMatcher, CompMatcher>();
        services.AddSingleton<IItemAdvisor, ItemAdvisor>();
        services.AddSingleton<IEconomyAdvisor, EconomyAdvisor>();
        services.AddSingleton<IAugmentAdvisor, AugmentAdvisor>();
        services.AddSingleton<IRecommendationEngine, RecommendationEngine>();

        // === 事件总线 ===
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // === 持久化 ===
        services.AddSingleton<IGameStateRepository>(sp =>
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TFTAssistant", "data.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            DatabaseInitializer.InitializeAsync(dbPath).Wait();
            return new SqliteGameStateRepository(dbPath);
        });

        services.AddSingleton<IMatchRepository>(sp =>
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TFTAssistant", "data.db");
            return new SqliteMatchRepository(dbPath);
        });

        // === Overwolf 集成 ===
        services.AddSingleton<OWBridge>();
        services.AddSingleton<OWWindowManager>();

        return services.BuildServiceProvider();
    }
}
```

#### 启动入口

```csharp
// src/TFTAssistant.App/Program.cs
namespace TFTAssistant.App;

using TFTAssistant.Core.Abstractions;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var provider = Bootstrapper.Build();

            // 初始化数据库
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TFTAssistant", "data.db");

            // 启动数据源
            var dataProvider = provider.GetRequiredService<CompositeGameDataProvider>();
            using var cts = new CancellationTokenSource();
            await dataProvider.StartAsync(cts.Token);

            // 初始化 Bridge
            var bridge = provider.GetRequiredService<OWBridge>();

            // 订阅游戏事件
            var eventBus = provider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<Core.Models.Events.GameStarted>(async e =>
            {
                Console.WriteLine($"[Event] 对局开始: {e.MatchId}");
            });

            eventBus.Subscribe<Core.Models.Events.GameEnded>(async e =>
            {
                Console.WriteLine($"[Event] 对局结束: 名次 #{e.Placement}");
            });

            Console.WriteLine("TFT Assistant 已启动");
            Console.WriteLine("等待 Overwolf 连接...");

            // 保持运行
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"启动失败: {ex}");
            throw;
        }
    }
}
```

---

## 12. 模块依赖关系

```
┌─────────────────────────────────────────────────────────┐
│                     TFTAssistant.App                     │
│                      (启动入口 + DI)                      │
└───────┬─────────────────────────────────┬───────────────┘
        │                                 │
        ▼                                 ▼
┌───────────────────┐         ┌───────────────────────┐
│ TFTAssistant.     │         │    前端 UI             │
│ Overwolf          │◄───────►│ (HTML/CSS/JS)         │
│ (集成层)          │  Bridge  │                       │
└───────┬───────────┘         └───────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────┐
│                 TFTAssistant.Core                      │
│                   (核心逻辑层)                           │
│                                                       │
│  ┌──────────┐  ┌──────────┐  ┌──────────────────┐    │
│  │  Data    │  │  Engine  │  │   Events         │    │
│  │ (数据层) │  │ (推荐)   │  │  (事件总线)       │    │
│  └────┬─────┘  └────┬─────┘  └──────────────────┘    │
│       │             │                                  │
│  ┌────▼─────────────▼─────┐                           │
│  │    Abstractions        │                           │
│  │    (接口定义层)         │                           │
│  └────────────────────────┘                           │
│       │                                                │
│  ┌────▼─────────────┐                                 │
│  │    Models        │                                 │
│  │  (数据模型层)     │                                 │
│  └──────────────────┘                                 │
│       │                                                │
│  ┌────▼─────────────┐                                 │
│  │    Storage       │                                 │
│  │  (持久化层)       │                                 │
│  └──────────────────┘                                 │
└───────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────┐
│                   外部依赖                              │
│  Live Client API │ Data Dragon │ Riot API │ Overwolf  │
│  (localhost:2999)│ (CDN)       │ (REST)   │ (SDK)     │
└───────────────────────────────────────────────────────┘

依赖规则:
  ✅ Core 不依赖 Overwolf 和 UI（可独立测试）
  ✅ Overwolf 依赖 Core（通过接口）
  ✅ UI 通过 Bridge 通信，不直接引用 Core
  ✅ App 负责组装所有模块
```

---

## 13. Riot 合规指南

### 13.1 核心原则

> 工具不能创造不公平优势、不能减少游戏决策多样性、不能剥夺玩家学习的能力。

### 13.2 功能合规矩阵

| 功能 | 合规性 | 说明 |
|------|--------|------|
| 棋盘/备战席/商店状态追踪 | ✅ 允许 | Live API 提供的数据 |
| 静态阵容推荐 | ✅ 允许 | 游戏前即可获得的 Meta 信息 |
| 装备合成表查询 | ✅ 允许 | 静态数据 |
| 通用经济策略提示 | ✅ 允许 | 通用规则，非实时动态 |
| 赛后对局分析 | ✅ 允许 | Riot API 历史数据 |
| 排位数据统计 | ✅ 允许 | 公开数据 |
| **实时动态建议**（"买这个"） | ❌ 禁止 | 根据实时局面调整的动态建议 |
| **对手棋盘侦察** | ❌ 禁止 | 追踪对手棋子 |
| **强化符文胜率** | ❌ 禁止 | 明确禁止显示 |
| **英雄池剩余追踪** | ⚠️ 灰色地带 | 建议不实现 |

### 13.3 合规设计原则

1. **推荐引擎输出的是"参考信息"而非"操作指令"**
   - ✅ "这套阵容在当前版本表现良好"（静态信息）
   - ❌ "你应该买这个棋子"（动态指令）

2. **经济提示基于通用规则，不根据实时局面动态调整**
   - ✅ "4阶段建议升到7级"（通用规则）
   - ❌ "你现在血量低，应该刷新"（根据实时血量的动态建议）

3. **不显示任何胜率数据**
   - ❌ "这个强化符文胜率 52%"（明确禁止）
   - ✅ "这个符文与你的阵容契合"（定性评价）

### 13.4 注册要求

- 所有面向玩家的产品必须在 [Riot Developer Portal](https://developer.riotgames.com/) 注册
- 必须有免费使用层级
- 个人项目可使用 Personal API Key

---

## 14. 开发路线图

### Phase 1 — MVP（1-2 周）

**目标**：实现基础的记牌器功能，能在游戏中显示当前棋盘状态

- [ ] 搭建解决方案和项目结构
- [ ] 实现 `GameState` 模型和 `GameStateDiff`
- [ ] 实现 `LiveClientDataProvider`（HTTP 轮询）
- [ ] 实现 `MockGameDataProvider`（用于无游戏时测试）
- [ ] 实现基础覆盖层 UI（棋盘 + 备战席 + 商店）
- [ ] 实现 `BridgeClient`（JS ↔ C# 通信）
- [ ] 实现 Tab 切换和基础样式
- [ ] 创建 Overwolf `manifest.json`
- [ ] 本地调试：在 Overwolf 中加载应用

**验收标准**：启动 Overwolf 应用后，进入 TFT 对局，覆盖层能实时显示棋盘上的棋子、备战席和商店内容。

### Phase 2 — 数据增强（2-3 周）

**目标**：完善数据层，加入静态数据和持久化

- [ ] 实现 `DataDragonProvider`（英雄/装备/特质数据）
- [ ] 实现 `InMemoryEventBus`
- [ ] 实现 SQLite 数据库初始化和 `SqliteGameStateRepository`
- [ ] 实现装备合成面板 UI
- [ ] 实现羁绊计算和展示
- [ ] 实现 Overwolf 事件监听（`OverwolfEventAdapter`）
- [ ] 实现 `CompositeGameDataProvider`（双数据源切换）
- [ ] 加入快捷键支持（显示/隐藏覆盖层）
- [ ] 加入连接状态指示器

**验收标准**：覆盖层能显示棋子的羁绊信息，装备面板能展示合成路径，Overwolf 事件和 Live API 双通道工作正常。

### Phase 3 — 推荐引擎（3-4 周）

**目标**：实现阵容匹配、装备建议、经济提示

- [ ] 实现 `CompMatcher`（阵容匹配算法）
- [ ] 实现 `ItemAdvisor`（装备建议）
- [ ] 实现 `EconomyAdvisor`（经济策略）
- [ ] 实现 `AugmentAdvisor`（强化符文评分）
- [ ] 实现 `RecommendationEngine`（总入口）
- [ ] 创建 Meta 阵容数据库（`meta-comps.json`）
- [ ] 创建装备推荐规则表（`ItemBuildDatabase`）
- [ ] 实现阵容建议面板 UI
- [ ] 实现经济提示面板 UI
- [ ] 合规审查：确保所有建议功能在政策边界内

**验收标准**：覆盖层能根据当前棋盘推荐 Top 3 阵容，给出装备分配建议和经济策略提示。

### Phase 4 — 赛后分析（2-3 周）

**目标**：实现对局历史记录和数据分析

- [ ] 实现 `RiotApiMatchProvider`
- [ ] 实现 `SqliteMatchRepository`
- [ ] 实现对局自动记录（监听 `GameEnded` 事件）
- [ ] 实现回合快照保存
- [ ] 实现桌面主窗口（对局历史页）
- [ ] 实现统计仪表盘（胜率/前四率/吃鸡率）
- [ ] 实现阵容统计（`comp_stats` 聚合）
- [ ] 实现设置页面（API Key 配置）

**验收标准**：每局结束后自动保存对局数据，桌面窗口能查看历史对局和统计数据。

### Phase 5 — 打磨优化（持续）

- [ ] 性能优化（减少不必要的 UI 重绘）
- [ ] 错误处理和日志完善
- [ ] 自动更新机制
- [ ] Overwolf 应用商店提交
- [ ] 用户反馈收集和迭代

---

## 15. 测试策略

### 15.1 单元测试

```csharp
// tests/TFTAssistant.Core.Tests/Engine/CompMatcherTests.cs
using Xunit;
using Moq;

public class CompMatcherTests
{
    private readonly Mock<IStaticDataProvider> _staticData = new();
    private readonly CompMatcher _matcher;

    public CompMatcherTests()
    {
        _matcher = new CompMatcher(
            _staticData.Object,
            Mock.Of<ILogger<CompMatcher>>());
    }

    [Fact]
    public async Task Match_WithEmptyBoard_ReturnsAllComps()
    {
        // Arrange
        _staticData.Setup(s => s.GetMetaCompsAsync("set13"))
            .ReturnsAsync(new List<MetaComp>
            {
                new() { Name = "Test Comp", Units = new() { ... } }
            });

        // Act
        var result = await _matcher.MatchAsync(
            new List<BoardUnit>(),
            new List<BenchUnit>(),
            new List<Augment>());

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Comp", result[0].Comp.Name);
    }

    [Fact]
    public async Task Match_WithMatchingBoard_ScoresHigher()
    {
        // Arrange: 棋盘上有阵容中的棋子，应该得分更高
        // ...
    }
}
```

### 15.2 集成测试

```csharp
// tests/TFTAssistant.Core.Tests/Data/LiveClientDataProviderTests.cs
public class LiveClientDataProviderTests
{
    [Fact]
    public async Task GetFullState_WithMockServer_ReturnsGameState()
    {
        // 使用 Mock HTTP Handler 模拟 Live API 响应
        // ...
    }
}
```

### 15.3 Mock 数据

```csharp
// src/TFTAssistant.Core/Data/MockGameDataProvider.cs
namespace TFTAssistant.Core.Data;

/// <summary>
/// Mock 数据源 —— 用于开发和测试
/// 生成模拟的游戏状态数据
/// </summary>
public sealed class MockGameDataProvider : IGameDataProvider
{
    private readonly Timer _timer;
    private int _round = 1;

    public bool IsConnected { get; private set; } = true;
    public DataSourceType Type => DataSourceType.Mock;
    public event EventHandler<GameStateDiff>? StateChanged;

    public MockGameDataProvider()
    {
        _timer = new Timer(async _ =>
        {
            var state = GenerateMockState();
            var diff = GameStateDiff.Compute(null, state);
            StateChanged?.Invoke(this, diff);
            _round++;
        }, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5));
    }

    private GameState GenerateMockState()
    {
        return new GameState
        {
            ActivePlayer = new ActivePlayer
            {
                SummonerName = "TestPlayer",
                Level = 6 + (_round / 10),
                CurrentGold = 30 + new Random().Next(0, 30),
                Health = 100 - (_round / 3) * 2,
                Experience = 0,
                TotalGold = 50,
                Placement = 1,
                Board = GenerateMockBoard(),
                Bench = GenerateMockBench(),
                Shop = GenerateMockShop()
            },
            AllPlayers = GenerateMockPlayers(),
            GameInfo = new GameInfo
            {
                GameTime = _round * 30.0,
                Round = _round % 6,
                Stage = 1 + (_round / 30),
                IsPvp = _round % 6 != 0,
                SetNumber = "Set13"
            },
            Augments = new List<Augment>()
        };
    }

    // ... GenerateMock* 方法省略

    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StopAsync() { _timer.Dispose(); return Task.CompletedTask; }
    public Task<GameState?> GetFullStateAsync() => Task.FromResult<GameState?>(null);
}
```

---

## 16. 部署与分发

### 16.1 Overwolf 应用打包

```bash
# 安装 Overwolf 打包工具
npm install -g @overwolf/ow-cli

# 打包应用
ow-cli pack --manifest docs/manifest.json --output dist/

# 输出: dist/TFTAssistant.opk
```

### 16.2 本地安装测试

1. 打开 Overwolf 客户端
2. 设置 → 开发者 → 加载未打包扩展
3. 选择项目根目录（包含 `manifest.json`）
4. 启动 TFT，验证覆盖层是否正常显示

### 16.3 Overwolf 应用商店发布

1. 在 Overwolf 开发者后台创建应用
2. 上传 `.opk` 包
3. 提交审核
4. 审核通过后上架

### 16.4 自动更新

Overwolf 平台内置自动更新机制，应用发布新版本后用户会自动收到更新。

---

## 附录 A: Live Client API 端点参考

| 端点 | 方法 | 说明 |
|------|------|------|
| `/liveclientdata/allgamedata` | GET | 获取所有游戏数据（推荐使用） |
| `/liveclientdata/playerlist` | GET | 所有玩家列表 |
| `/liveclientdata/activeplayer` | GET | 当前玩家数据 |
| `/liveclientdata/activeplayername` | GET | 当前玩家名称 |
| `/liveclientdata/gamestats` | GET | 游戏统计 |
| `/liveclientdata/events` | GET | 游戏事件列表 |

**Base URL**: `https://127.0.0.1:2999`

**注意**:
- 使用自签名证书，HTTP 客户端需忽略 SSL 验证
- 仅在游戏运行时可用
- 无需 API Key

---

## 附录 B: Overwolf TFT 事件参考

**游戏 ID**: `5426`

### 声明所需功能

```javascript
overwolf.games.events.setRequiredFeatures([
    'live_client_data',
    'match_info',
    'game_info',
    'match_state'
], console.log);
```

### 主要事件

| 事件名 | 触发时机 | 数据内容 |
|--------|----------|----------|
| `match_info` | 对局信息更新 | 回合类型、战斗状态 |
| `match_state` | 对局状态变化 | 等待中/进行中/结束 |
| `live_client_data` | 实时数据更新 | 棋盘、商店、血量等 |
| `game_info` | 游戏信息更新 | 游戏时间、版本 |

### 信息更新 (info_update)

| 字段路径 | 说明 |
|----------|------|
| `info.live_client_data.allPlayers` | 所有玩家 |
| `info.live_client_data.activePlayer` | 当前玩家 |
| `info.live_client_data.gameData` | 游戏数据 |
| `info.match_info` | 对局信息 |
| `info.game_info` | 游戏信息 |

---

## 附录 C: Data Dragon 数据结构

### 英雄数据 (tft-champion.json)

```json
{
  "type": "champion",
  "version": "15.1.1",
  "data": {
    "TFT13_Ahri": {
      "name": "Ahri",
      "cost": 3,
      "traits": ["TFT13_Arcanist", "TFT13_Spellcraft"],
      "ability": {
        "name": "Spirit Rush",
        "description": "..."
      },
      "stats": {
        "attack_damage": 50,
        "health": 600,
        "armor": 20,
        "magic_resist": 20
      }
    }
  }
}
```

### 装备数据 (tft-item.json)

```json
{
  "type": "item",
  "version": "15.1.1",
  "data": {
    "TFT_Item_BFSword": {
      "name": "B.F. Sword",
      "description": "+15 Attack Damage",
      "isComponent": true,
      "from": [],
      "icon": "..."
    },
    "TFT_Item_InfinityEdge": {
      "name": "Infinity Edge",
      "description": "+30% Critical Strike Damage",
      "isComponent": false,
      "from": ["TFT_Item_BFSword", "TFT_Item_Cloak"],
      "icon": "..."
    }
  }
}
```

---

## 附录 D: HDT 架构映射对照表

| HDT 模块 | HDT 技术 | TFT Assistant 对应 | TFT 技术 | 变化说明 |
|----------|----------|-------------------|----------|----------|
| `LogFileWatcher` | 文件监控 + 正则 | `LiveClientDataProvider` | HTTP 轮询 | **更简单** |
| `HearthMirror` | DLL 注入 + IPC | `OverwolfEventAdapter` | OW SDK | **更安全** |
| `PowerHandler` | 日志行解析 | `GameStateDiff` | JSON diff | **更稳定** |
| `TagChangeHandler` | 标签变化处理 | `EventBus` | 事件总线 | 架构升级 |
| `GameV2` | 游戏状态 | `GameState` | POCO | 概念一致 |
| `Entity` | 游戏实体 | `BoardUnit/BenchUnit/ShopUnit` | POCO | 按 TFT 拆分 |
| `HearthDb` | 卡牌数据库 | `DataDragonProvider` | HTTP CDN | 数据源不同 |
| `OverlayWindow` | WPF 透明窗口 | Overwolf Transparent Window | OW SDK | **无需手写** |
| `decks.xml` | XML 持久化 | `SQLite` | 关系型数据库 | **更强大** |
| `Squirrel.Windows` | 自动更新 | Overwolf 内置 | 平台内置 | **无需自实现** |
| ❌ 无 | — | `RecommendationEngine` | C# | **新增模块** |
| ❌ 无 | — | `Analytics` | C# | **新增模块** |
| ❌ 无 | — | `IEventBus` | C# | **新增模块** |

---

> **文档版本**: 1.0
> **最后更新**: 2026-04-12
> **适用赛季**: Set 13+
