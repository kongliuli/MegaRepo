# TFT Assistant - Product Requirement Document

## Overview
- **Summary**: 基于 Hearthstone Deck Tracker (HDT) 架构思想，使用 Riot Live Client Data API + Overwolf SDK 构建的云顶之弈个人辅助工具。实现游戏状态追踪、智能建议和赛后分析功能。
- **Purpose**: 为云顶之弈玩家提供实时的游戏状态追踪、阵容匹配建议、装备分配参考和经济策略提示，同时遵守 Riot 第三方工具政策。
- **Target Users**: 云顶之弈（Teamfight Tactics）玩家，特别是希望提升游戏表现的休闲和进阶玩家。

## Goals
- 实时追踪棋盘、备战席、商店、经济等游戏状态
- 提供基于 Meta 阵容的智能推荐（阵容匹配、装备建议、经济提示）
- 支持赛后对局历史记录和数据分析
- 严格遵守 Riot 第三方工具政策
- 采用仿 HDT 的分层解耦架构，便于维护和扩展

## Non-Goals (Out of Scope)
- 不提供对手棋盘侦察功能（禁止）
- 不提供实时动态操作指令（"买这个"类建议）
- 不显示胜率数据（Riot 明确禁止）
- 不追踪英雄池剩余数量（灰色地带）
- 不开发移动端应用
- 不提供多人协作功能

## Background & Context
- 参考 Hearthstone Deck Tracker (HDT) 的成功架构和设计理念
- 使用 .NET 8.0 LTS 作为后端技术栈
- 集成 Overwolf SDK 实现游戏内覆盖层
- 通过 Riot Live Client Data API (localhost:2999) 获取实时游戏数据
- 通过 Data Dragon 获取静态游戏数据
- 通过 Riot API 获取赛后历史数据
- 技术选型优先考虑合规性、可维护性和可测试性

## Functional Requirements
- **FR-1**: 游戏状态实时追踪（棋盘、备战席、商店、经济、血量、等级等）
- **FR-2**: 阵容匹配推荐（基于当前棋盘匹配最优 Meta 阵容）
- **FR-3**: 装备建议（根据英雄和可用组件推荐装备分配）
- **FR-4**: 经济策略提示（基于通用规则的运营建议）
- **FR-5**: 强化符文评分（基于静态规则的定性评价）
- **FR-6**: 赛后对局历史记录（自动保存对局数据）
- **FR-7**: 统计分析（胜率、前四率、阵容表现等）
- **FR-8**: 游戏内覆盖层 UI（实时显示信息）
- **FR-9**: 桌面主窗口（历史记录和统计）
- **FR-10**: 双数据源支持（Live Client API + Overwolf 事件）

## Non-Functional Requirements
- **NFR-1**: 实时性：状态更新延迟不超过 1 秒
- **NFR-2**: 性能：UI 响应流畅，无明显卡顿
- **NFR-3**: 可测试性：核心逻辑层无 UI 依赖，可独立测试
- **NFR-4**: 可维护性：采用分层架构，模块间通过接口解耦
- **NFR-5**: 合规性：所有功能在 Riot 政策边界内设计
- **NFR-6**: 稳定性：异常有适当处理，不影响游戏体验

## Constraints
- **Technical**: 
  - 后端使用 .NET 8.0 LTS + C# 12.0
  - 前端使用原生 HTML/CSS/JS（Overwolf 窗口内）
  - 数据库使用 SQLite
  - 必须通过 Overwolf SDK 集成
- **Business**:
  - 必须遵守 Riot 第三方工具政策
  - 必须有免费使用层级
  - 个人项目使用 Personal API Key
- **Dependencies**:
  - Riot Live Client Data API
  - Riot Data Dragon
  - Riot Developer API（可选，用于赛后数据）
  - Overwolf SDK

## Assumptions
- 用户已安装 Overwolf 客户端
- TFT 游戏运行时 Live Client API 可用 (localhost:2999)
- Data Dragon CDN 可访问
- 用户愿意提供 Riot API Key（用于赛后分析功能）
- 用户理解推荐仅供参考，最终决策由玩家自己做出

## Acceptance Criteria

### AC-1: 实时游戏状态追踪
- **Given**: 用户正在进行 TFT 对局
- **When**: 应用启动并连接到 Live Client API
- **Then**: 覆盖层实时显示棋盘上的棋子、备战席、商店、当前金币、血量、等级等信息
- **Verification**: `programmatic`
- **Notes**: 状态更新延迟不超过 1 秒

### AC-2: 阵容匹配推荐
- **Given**: 用户棋盘上有若干棋子
- **When**: 用户查看阵容推荐面板
- **Then**: 显示 Top 3 最匹配的 Meta 阵容，标注已拥有和缺少的棋子
- **Verification**: `programmatic`
- **Notes**: 推荐基于静态 Meta 数据，不根据实时局面动态调整

### AC-3: 装备建议
- **Given**: 用户棋盘上有 2 星及以上棋子，且有可用装备组件
- **When**: 用户查看装备面板
- **Then**: 显示适合的装备分配建议，按优先级排序
- **Verification**: `programmatic`
- **Notes**: 基于静态装备推荐规则表

### AC-4: 经济策略提示
- **Given**: 用户处于游戏对局中
- **When**: 用户查看经济面板
- **Then**: 显示基于通用规则的经济策略提示（如"前期尽量保持 50 金吃利息"）
- **Verification**: `programmatic`
- **Notes**: 提示基于通用规则，不根据实时局面动态调整

### AC-5: 强化符文评分
- **Given**: 用户面临强化符文选择
- **When**: 符文选择界面出现
- **Then**: 对可选符文进行定性评分和说明，不显示胜率数据
- **Verification**: `programmatic`
- **Notes**: 评分基于契合度和通用强度，无胜率数据

### AC-6: 赛后历史记录
- **Given**: 用户完成一局 TFT 对局
- **When**: 对局结束
- **Then**: 应用自动保存对局记录到本地数据库
- **Verification**: `programmatic`

### AC-7: 统计分析
- **Given**: 用户有历史对局记录
- **When**: 用户打开桌面主窗口的统计页面
- **Then**: 显示胜率、前四率、吃鸡率、阵容表现等统计数据
- **Verification**: `programmatic`

### AC-8: 覆盖层 UI
- **Given**: 用户正在进行 TFT 对局
- **When**: 应用运行
- **Then**: 游戏内显示透明覆盖层，可拖拽、可切换标签页
- **Verification**: `human-judgment`
- **Notes**: UI 设计美观、响应流畅

### AC-9: 合规性
- **Given**: 应用所有功能已实现
- **When**: 进行合规审查
- **Then**: 确认无禁止功能（对手侦察、动态指令、胜率数据等）
- **Verification**: `human-judgment`

## Open Questions
- [ ] 是否需要实现 Overwolf 事件作为 Live API 的备用数据源？
- [ ] Meta 阵容数据和装备推荐规则的更新机制是什么？
- [ ] 是否需要支持多赛季数据切换？
