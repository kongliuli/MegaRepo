# TFT Assistant - 数据集成与胜率查询 - The Implementation Plan (Decomposed and Prioritized Task List)

## [x] Task 1: 完善 Overwolf 事件适配器实现
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 完善 OverwolfEventAdapter 类的实现
  - 实现 OWInfoUpdateMapper.ToGameState 方法
  - 实现 Overwolf 事件数据到 GameState 的完整映射
  - 处理 Overwolf 提供的所有游戏事件
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-1.1: OverwolfEventAdapter 能正确处理 info_update 事件
  - `programmatic` TR-1.2: OWInfoUpdateMapper 能正确映射到 GameState
  - `programmatic` TR-1.3: 能触发 StateChanged 事件
- **Notes**: 参考开发指南中的 OverwolfEventAdapter 代码

## [x] Task 2: 实现组合数据源 (CompositeGameDataProvider)
- **Priority**: P0
- **Depends On**: Task 1
- **Description**: 
  - 实现 CompositeGameDataProvider 类
  - 实现双数据源自动切换逻辑
  - 实现数据源健康检查
  - 支持用户手动选择数据源
- **Acceptance Criteria Addressed**: AC-1
- **Test Requirements**:
  - `programmatic` TR-2.1: 能自动检测 Live API 是否可用
  - `programmatic` TR-2.2: Live API 不可用时自动切换到 Overwolf
  - `programmatic` TR-2.3: 两个数据源都正常时优先使用 Live API
- **Notes**: 确保数据格式统一，对上层透明

## [x] Task 3: 扩展 DataDragonProvider 支持多版本
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 修改 DataDragonProvider 以支持多版本
  - 实现版本列表获取
  - 实现按版本号加载数据
  - 更新本地缓存目录结构（按版本组织）
- **Acceptance Criteria Addressed**: AC-2
- **Test Requirements**:
  - `programmatic` TR-3.1: 能获取可用版本列表
  - `programmatic` TR-3.2: 能加载指定版本的英雄数据
  - `programmatic` TR-3.3: 能加载指定版本的装备数据
- **Notes**: 保持向后兼容，默认使用最新版本

## [x] Task 4: 实现静态数据缓存管理
- **Priority**: P0
- **Depends On**: Task 3
- **Description**: 
  - 实现缓存状态查询（已下载哪些版本）
  - 实现缓存清理功能
  - 实现缓存过期策略
  - 添加缓存管理 API
- **Acceptance Criteria Addressed**: AC-3
- **Test Requirements**:
  - `programmatic` TR-4.1: 能查询已缓存的版本列表
  - `programmatic` TR-4.2: 能清理指定版本的缓存
  - `programmatic` TR-4.3: 缓存命中时直接返回本地数据
- **Notes**: 提供进度反馈，避免长时间阻塞

## [x] Task 5: 扩展数据库模型支持胜率统计
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - 扩展 MatchRecord 模型，添加阵容信息
  - 创建 CompStats 模型（阵容统计）
  - 更新数据库初始化脚本
  - 实现数据库迁移
- **Acceptance Criteria Addressed**: AC-4, AC-5
- **Test Requirements**:
  - `programmatic` TR-5.1: 数据库能正确创建新表
  - `programmatic` TR-5.2: 能保存阵容信息到对局记录
  - `programmatic` TR-5.3: 数据库迁移能正常工作
- **Notes**: 保留现有数据，支持平滑迁移

## [x] Task 6: 实现胜率统计服务
- **Priority**: P0
- **Depends On**: Task 5
- **Description**: 
  - 实现胜率查询接口
  - 实现总体胜率统计（胜率、前四率、吃鸡率）
  - 实现按阵容胜率统计
  - 实现按版本胜率统计
- **Acceptance Criteria Addressed**: AC-4, AC-5
- **Test Requirements**:
  - `programmatic` TR-6.1: 能计算总体胜率统计
  - `programmatic` TR-6.2: 能查询特定阵容的胜率
  - `programmatic` TR-6.3: 能按版本过滤胜率数据
- **Notes**: 确保性能，支持缓存统计结果

## [x] Task 7-12: 为所有新模块编写单元测试
- **Priority**: P0
- **Depends On**: Tasks 1-6
- **Description**: 
  - 为 OverwolfEventAdapter 编写单元测试
  - 为 CompositeGameDataProvider 编写单元测试
  - 为多版本 DataDragonProvider 编写单元测试
  - 为 WinRateStatisticsService 编写单元测试
  - 为数据库仓储编写单元测试
- **Acceptance Criteria Addressed**: AC-8
- **Test Requirements**:
  - `programmatic` TR-All: 所有单元测试通过
- **Notes**: 使用 Moq 模拟依赖，覆盖主要分支逻辑

## [x] Task 13: 实现数据管理 UI
- **Priority**: P1
- **Depends On**: Task 4
- **Description**: 
  - 创建数据管理页面 UI
  - 实现版本选择器
  - 实现缓存管理界面
  - 实现数据下载进度显示
- **Acceptance Criteria Addressed**: AC-6
- **Test Requirements**:
  - `human-judgement` TR-7.1: 能正常显示版本列表
  - `human-judgement` TR-7.2: 能切换不同版本
  - `human-judgement` TR-7.3: 缓存管理功能正常
- **Notes**: 在桌面主窗口中实现

## [x] Task 14: 实现胜率展示 UI
- **Priority**: P1
- **Depends On**: Task 6
- **Description**: 
  - 创建胜率统计页面 UI
  - 实现总体胜率展示（图表 + 数字）
  - 实现阵容胜率列表
  - 添加明显的"历史数据"标识
- **Acceptance Criteria Addressed**: AC-7
- **Test Requirements**:
  - `human-judgement` TR-8.1: 能正常显示胜率统计
  - `human-judgement` TR-8.2: 有明显的历史数据标识
  - `human-judgement` TR-8.3: UI 响应流畅
- **Notes**: 在桌面主窗口中实现

## [x] Task 15: 合规性审查和最终测试
- **Priority**: P0
- **Depends On**: All previous tasks
- **Description**: 
  - 进行完整的合规性审查
  - 确认胜率展示明确标明为历史数据
  - 确认无实时预测功能
  - 进行端到端测试
  - 性能优化
- **Acceptance Criteria Addressed**: AC-9, All ACs
- **Test Requirements**:
  - `human-judgement` TR-15.1: 确认胜率展示有明显历史数据标识
  - `programmatic` TR-15.2: 所有单元测试通过
  - `human-judgement` TR-15.3: 端到端测试通过
- **Notes**: 这是发布前的最终检查
