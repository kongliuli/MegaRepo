# TFT Assistant - The Implementation Plan (Decomposed and Prioritized Task List)

## [x] Task 1: 创建解决方案和项目结构
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 创建 .NET 8 解决方案 TFTAssistant.sln
  - 创建核心类库项目 TFTAssistant.Core
  - 创建 Overwolf 集成类库项目 TFTAssistant.Overwolf
  - 创建启动控制台应用项目 TFTAssistant.App
  - 创建测试项目 TFTAssistant.Core.Tests
  - 配置项目引用关系
  - 安装必要的 NuGet 包（Serilog, Microsoft.Data.Sqlite 等）
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-1.1: 解决方案能成功编译
  - `programmatic` TR-1.2: 项目引用关系正确配置
- **Notes**: 遵循开发指南中的目录结构

## [x] Task 2: 实现游戏状态模型
- **Priority**: P0
- **Depends On**: Task 1
- **Description**: 
  - 创建 Models/Game 命名空间
  - 实现 GameState 模型（游戏状态根对象）
  - 实现 ActivePlayer 模型（当前玩家状态）
  - 实现 BoardUnit, BenchUnit, ShopUnit 模型
  - 实现 PlayerSummary, GameInfo, Augment 模型
  - 实现 GameStateDiff 模型（状态差异计算）
  - 实现所有模型的 IEquatable&lt;T&gt; 接口
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-2.1: GameStateDiff.Compute 能正确计算状态差异
  - `programmatic` TR-2.2: 所有模型的 Equals 和 GetHashCode 正确实现
- **Notes**: 参考开发指南中的代码示例

## [x] Task 3: 实现 Live Client Data Provider
- **Priority**: P0
- **Depends On**: Task 2
- **Description**: 
  - 创建 Abstractions 命名空间和接口定义
  - 实现 IGameDataSource 和 IGameDataProvider 接口
  - 实现 LiveClientDataProvider 类（HTTP 轮询）
  - 实现 LiveGameDataDto（API 响应 DTO 和映射）
  - 配置 HttpClient 以支持自签名证书
  - 实现状态变更事件触发
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-3.1: LiveClientDataProvider 能启动和停止
  - `programmatic` TR-3.2: 能正确解析 API 响应并转换为 GameState
  - `programmatic` TR-3.3: 状态变更时能触发 StateChanged 事件
- **Notes**: 使用 mock HTTP 服务器进行测试

## [x] Task 4: 实现 Mock Game Data Provider
- **Priority**: P0
- **Depends On**: Task 3
- **Description**: 
  - 实现 MockGameDataProvider 类
  - 生成模拟的游戏状态数据
  - 定时更新状态以模拟游戏进行
  - 支持无游戏环境下的开发和测试
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-4.1: Mock 数据源能生成有效的 GameState
  - `programmatic` TR-4.2: 能定时触发状态变更事件
- **Notes**: 用于 UI 开发和集成测试

## [x] Task 5: 实现基础覆盖层 UI
- **Priority**: P0
- **Depends On**: Task 4
- **Description**: 
  - 创建 src/ui/overlay 目录结构
  - 实现 index.html（覆盖层主页面）
  - 实现 CSS 样式（reset.css, variables.css, overlay.css）
  - 实现 Tab 切换功能
  - 实现棋盘追踪面板 UI
  - 实现基础样式和响应式布局
- **Acceptance Criteria Addressed**: AC-1, AC-8
- **Test Requirements**:
  - `human-judgement` TR-5.1: 覆盖层 UI 能在浏览器中正常显示
  - `human-judgement` TR-5.2: Tab 切换功能正常工作
  - `human-judgement` TR-5.3: UI 设计美观，风格统一
- **Notes**: 先在普通浏览器中测试，再集成到 Overwolf

## [x] Task 6: 实现 C# ↔ JS 通信桥
- **Priority**: P0
- **Depends On**: Task 5
- **Description**: 
  - 实现 OWBridge 类（C# 端）
  - 实现 bridge-client.js（JS 端）
  - 实现 MessageTypes.cs（消息类型定义）
  - 实现状态变更推送
  - 实现 UI 消息处理
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-6.1: C# 端能发送消息到 JS 端
  - `programmatic` TR-6.2: JS 端能发送消息到 C# 端
  - `programmatic` TR-6.3: 状态变更能正确推送到 UI
- **Notes**: 可使用 mock 数据测试桥接通信

## [x] Task 7: 创建 Overwolf manifest.json
- **Priority**: P0
- **Depends On**: Task 6
- **Description**: 
  - 创建 docs/manifest.json
  - 配置应用基本信息（名称、版本、权限等）
  - 配置窗口（overlay, desktop, background）
  - 配置游戏事件监听
  - 配置快捷键
- **Acceptance Criteria Addressed**: AC-1, AC-8
- **Test Requirements**:
  - `human-judgement` TR-7.1: manifest.json 格式正确
  - `human-judgement` TR-7.2: 能在 Overwolf 开发者模式中加载
- **Notes**: 参考开发指南中的 manifest.json 示例

## [ ] Task 8: 实现 Data Dragon Provider
- **Priority**: P1
- **Depends On**: Task 3
- **Description**: 
  - 创建静态数据模型（Champion, Item, Trait, MetaComp 等）
  - 实现 IStaticDataProvider 接口
  - 实现 DataDragonProvider 类
  - 实现从 Data Dragon CDN 下载数据
  - 实现本地缓存机制
  - 实现 JSON 解析
- **Acceptance Criteria Addressed**: AC-2, AC-3
- **Test Requirements**:
  - `programmatic` TR-8.1: 能成功下载并解析英雄数据
  - `programmatic` TR-8.2: 能成功下载并解析装备数据
  - `programmatic` TR-8.3: 本地缓存功能正常工作
- **Notes**: 优先从本地文件加载，失败时从 CDN 下载

## [ ] Task 9: 实现事件总线
- **Priority**: P1
- **Depends On**: Task 2
- **Description**: 
  - 创建事件模型（GameStarted, GameEnded, BoardChanged 等）
  - 实现 IEventBus 接口
  - 实现 InMemoryEventBus 类
  - 实现发布/订阅机制
- **Acceptance Criteria Addressed**: AC-1, AC-6
- **Test Requirements**:
  - `programmatic` TR-9.1: 事件能正确发布
  - `programmatic` TR-9.2: 订阅者能收到事件
  - `programmatic` TR-9.3: 取消订阅后不再收到事件
- **Notes**: 使用 ConcurrentDictionary 保证线程安全

## [ ] Task 10: 实现 SQLite 持久化
- **Priority**: P1
- **Depends On**: Task 9
- **Description**: 
  - 实现 DatabaseInitializer 类（数据库表创建）
  - 实现 IGameStateRepository 接口
  - 实现 SqliteGameStateRepository 类
  - 实现 IMatchRepository 接口
  - 实现 SqliteMatchRepository 类
  - 实现数据库路径配置
- **Acceptance Criteria Addressed**: AC-6, AC-7
- **Test Requirements**:
  - `programmatic` TR-10.1: 数据库能正确初始化
  - `programmatic` TR-10.2: 能保存和加载 GameState
  - `programmatic` TR-10.3: 能保存和查询对局记录
- **Notes**: 使用 Microsoft.Data.Sqlite

## [ ] Task 11: 实现阵容匹配器
- **Priority**: P1
- **Depends On**: Task 8
- **Description**: 
  - 实现 ICompMatcher 接口
  - 实现 CompMatcher 类
  - 实现阵容评分算法
  - 创建 Meta 阵容数据库（meta-comps.json）
  - 实现羁绊计算
- **Acceptance Criteria Addressed**: AC-2
- **Test Requirements**:
  - `programmatic` TR-11.1: 空棋盘时返回所有阵容
  - `programmatic` TR-11.2: 匹配的棋子越多得分越高
  - `programmatic` TR-11.3: 返回 Top 3 阵容
- **Notes**: 评分基于拥有棋子、羁绊激活度、强化符文契合度、版本强度

## [ ] Task 12: 实现装备建议器
- **Priority**: P1
- **Depends On**: Task 8
- **Description**: 
  - 实现 IItemAdvisor 接口
  - 实现 ItemAdvisor 类
  - 创建装备推荐规则表（ItemBuildDatabase）
  - 实现装备分配算法
  - 实现装备合成路径展示
- **Acceptance Criteria Addressed**: AC-3
- **Test Requirements**:
  - `programmatic` TR-12.1: 能为 2 星及以上棋子推荐装备
  - `programmatic` TR-12.2: 推荐按优先级排序
  - `programmatic` TR-12.3: 组件不会被重复分配
- **Notes**: 主 C 优先于副 C

## [ ] Task 13: 实现经济顾问
- **Priority**: P1
- **Depends On**: Task 2
- **Description**: 
  - 实现 IEconomyAdvisor 接口
  - 实现 EconomyAdvisor 类
  - 实现基于通用规则的经济策略
  - 确保不根据实时局面动态调整
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `programmatic` TR-13.1: 低血量时显示紧急提示
  - `programmatic` TR-13.2: 50 金以上提示利息已满
  - `programmatic` TR-13.3: 4 阶段提示升级
- **Notes**: 所有提示基于通用规则，确保合规

## [ ] Task 14: 实现强化符文评分
- **Priority**: P1
- **Depends On**: Task 8
- **Description**: 
  - 实现 IAugmentAdvisor 接口
  - 实现 AugmentAdvisor 类
  - 实现符文评分算法
  - 确保不显示胜率数据
  - 只提供定性评价
- **Acceptance Criteria Addressed**: AC-5
- **Test Requirements**:
  - `programmatic` TR-14.1: 能对可选符文进行评分
  - `programmatic` TR-14.2: 评分在 0-100 范围内
  - `programmatic` TR-14.3: 不包含胜率数据
- **Notes**: 评分基于契合度和通用强度

## [ ] Task 15: 实现推荐引擎总入口
- **Priority**: P1
- **Depends On**: Tasks 11, 12, 13, 14
- **Description**: 
  - 实现 IRecommendationEngine 接口
  - 实现 RecommendationEngine 类
  - 协调各子引擎
  - 并行调用提高性能
  - 统一异常处理
- **Acceptance Criteria Addressed**: AC-2, AC-3, AC-4, AC-5
- **Test Requirements**:
  - `programmatic` TR-15.1: 能返回完整的 RecommendationSet
  - `programmatic` TR-15.2: 异常时返回空结果而非崩溃
- **Notes**: 推荐是参考信息，不是操作指令

## [ ] Task 16: 实现阵容、装备、经济面板 UI
- **Priority**: P1
- **Depends On**: Tasks 5, 15
- **Description**: 
  - 实现阵容建议面板 UI（comp-panel.js）
  - 实现装备面板 UI（item-panel.js）
  - 实现经济提示面板 UI（econ-panel.js）
  - 实现强化符文面板 UI（augment-panel.js）
  - 更新 CSS 样式
- **Acceptance Criteria Addressed**: AC-2, AC-3, AC-4, AC-5, AC-8
- **Test Requirements**:
  - `human-judgement` TR-16.1: 各面板能正常显示推荐数据
  - `human-judgement` TR-16.2: UI 响应流畅
  - `human-judgement` TR-16.3: 样式美观统一
- **Notes**: 可使用 mock 数据测试 UI

## [ ] Task 17: 实现 Riot API 赛后数据
- **Priority**: P2
- **Depends On**: Task 10
- **Description**: 
  - 实现 IMatchHistoryProvider 接口
  - 实现 RiotApiMatchProvider 类
  - 实现速率限制
  - 实现 PUUID 获取
  - 实现对局列表和详情获取
- **Acceptance Criteria Addressed**: AC-6, AC-7
- **Test Requirements**:
  - `programmatic` TR-17.1: 能通过 Riot ID 获取 PUUID
  - `programmatic` TR-17.2: 能获取近期对局列表
  - `programmatic` TR-17.3: 速率限制正常工作
- **Notes**: 需要用户配置 API Key

## [ ] Task 18: 实现桌面主窗口
- **Priority**: P2
- **Depends On**: Task 17
- **Description**: 
  - 创建 src/ui/desktop 目录结构
  - 实现桌面主窗口 index.html
  - 实现对局历史页面（history.js）
  - 实现统计仪表盘页面（stats.js）
  - 实现设置页面（settings.js）
  - 实现 CSS 样式
- **Acceptance Criteria Addressed**: AC-6, AC-7, AC-8
- **Test Requirements**:
  - `human-judgement` TR-18.1: 能显示对局历史
  - `human-judgement` TR-18.2: 能显示统计数据
  - `human-judgement` TR-18.3: 设置页面能正常工作
- **Notes**: 桌面窗口用于历史记录和统计

## [ ] Task 19: 实现 Overwolf 事件适配器
- **Priority**: P2
- **Depends On**: Task 6
- **Description**: 
  - 实现 OverwolfEventAdapter 类
  - 实现 CompositeGameDataProvider 类
  - 实现双数据源切换
  - 实现 JS 端 Overwolf 事件监听
- **Acceptance Criteria Addressed**: AC-1, FR-10
- **Test Requirements**:
  - `programmatic` TR-19.1: Overwolf 事件能正确处理
  - `programmatic` TR-19.2: 数据源能自动切换
- **Notes**: 作为 Live API 的备用数据源

## [ ] Task 20: 合规审查和最终测试
- **Priority**: P0
- **Depends On**: All previous tasks
- **Description**: 
  - 进行完整的合规审查
  - 确认无禁止功能
  - 进行端到端测试
  - 性能优化
  - 错误处理完善
  - 日志完善
- **Acceptance Criteria Addressed**: AC-9, All ACs
- **Test Requirements**:
  - `human-judgement` TR-20.1: 确认无禁止功能
  - `programmatic` TR-20.2: 所有单元测试通过
  - `human-judgement` TR-20.3: 端到端测试通过
- **Notes**: 这是发布前的最终检查
