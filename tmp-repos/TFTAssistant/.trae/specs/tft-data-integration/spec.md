# TFT Assistant - 数据集成与胜率查询 - Product Requirement Document

## Overview
- **Summary**: 完善 TFT Assistant 的数据获取体系，实现 Overwolf 实时数据集成、多版本静态数据管理、以及胜率查询和匹配功能，同时确保符合 Riot 第三方工具政策。
- **Purpose**: 构建完整、可靠的数据来源体系，为玩家提供更全面的游戏辅助信息，提升工具的实用性和用户体验。
- **Target Users**: 云顶之弈（Teamfight Tactics）玩家，特别是希望获取更全面游戏数据和分析的玩家。

## Goals
- 实现 Overwolf 事件数据的完整集成和整理
- 实现多版本（当前版本和历史版本）静态数据的获取和缓存管理
- 实现合理的胜率查询和匹配功能（在 Riot 政策允许范围内）
- 基于获取的数据设计和实现对应的 UI 页面
- 为新功能编写完整的单元测试

## Non-Goals (Out of Scope)
- 不提供实时胜率预测（Riot 明确禁止）
- 不提供对手胜率分析（灰色地带）
- 不提供动态胜率计算（禁止）
- 不修改原始 spec.md 中已明确禁止的功能

## Background & Context
- 原始项目已实现 Live Client API 数据获取，需要补充 Overwolf 事件数据源
- 需要支持多赛季静态数据管理，便于玩家了解历史版本和版本切换
- 胜率查询功能需要在 Riot 政策边界内设计，采用基于历史统计的胜率展示，而非实时预测
- 参考项目已有的分层架构和设计理念

## Functional Requirements
- **FR-1**: Overwolf 实时数据集成（作为 Live Client API 的补充/备用数据源）
- **FR-2**: 多版本静态数据获取和缓存（当前版本 + 历史版本）
- **FR-3**: 胜率查询和匹配（基于历史统计数据）
- **FR-4**: 数据管理 UI（版本切换、数据缓存管理）
- **FR-5**: 历史胜率展示 UI（基于历史对局统计）
- **FR-6**: 单元测试覆盖（数据模块的完整测试）

## Non-Functional Requirements
- **NFR-1**: 数据可靠性：双数据源自动切换，确保数据不中断
- **NFR-2**: 性能：静态数据加载延迟 < 2 秒，缓存命中响应 < 100ms
- **NFR-3**: 可扩展性：易于添加新的赛季版本数据
- **NFR-4**: 合规性：胜率展示明确标明为历史统计，不包含实时预测
- **NFR-5**: 可测试性：数据模块可独立测试，依赖通过接口解耦

## Constraints
- **Technical**: 
  - 后端继续使用 .NET 8.0 LTS + C# 12.0
  - 数据库继续使用 SQLite
  - 必须通过 Overwolf SDK 集成
  - 胜率数据必须基于本地历史统计，不能调用外部胜率 API
- **Business**:
  - 必须遵守 Riot 第三方工具政策
  - 胜率展示必须明确标明为历史数据，非实时预测
- **Dependencies**:
  - Overwolf SDK TFT 事件 API
  - Riot Data Dragon（多版本支持）
  - 本地历史对局数据库

## Assumptions
- Overwolf 事件 API 能提供足够的游戏状态信息
- Data Dragon 提供历史版本数据的访问
- 用户理解胜率数据为历史统计，不代表当前对局结果
- 本地数据库存储足够的历史对局数据用于胜率统计

## Acceptance Criteria

### AC-1: Overwolf 实时数据集成
- **Given**: 用户正在进行 TFT 对局且 Overwolf 客户端运行
- **When**: Live Client API 不可用或用户选择 Overwolf 数据源
- **Then**: 应用自动切换到 Overwolf 事件数据源，正常获取游戏状态
- **Verification**: `programmatic`
- **Notes**: 支持双数据源自动切换，数据格式统一

### AC-2: 多版本静态数据获取
- **Given**: 用户需要查看或使用不同赛季的静态数据
- **When**: 用户选择不同的赛季版本
- **Then**: 应用能正确加载对应版本的英雄、装备、特质等静态数据
- **Verification**: `programmatic`
- **Notes**: 支持本地缓存，优先从缓存加载

### AC-3: 静态数据缓存管理
- **Given**: 应用已下载过某些版本的静态数据
- **When**: 用户再次请求相同版本的数据
- **Then**: 应用从本地缓存加载，无需重新下载
- **Verification**: `programmatic`
- **Notes**: 支持缓存清理和版本管理

### AC-4: 历史胜率查询
- **Given**: 用户有足够的历史对局记录
- **When**: 用户查看胜率统计页面
- **Then**: 显示基于历史数据的胜率、前四率、吃鸡率等统计，明确标明为历史数据
- **Verification**: `programmatic`
- **Notes**: 无实时预测，仅展示历史统计

### AC-5: 阵容胜率匹配
- **Given**: 用户棋盘上有特定阵容，且有历史对局记录
- **When**: 用户查看阵容胜率
- **Then**: 显示该阵容在历史对局中的表现统计，明确标明为历史数据
- **Verification**: `programmatic`
- **Notes**: 无实时预测，仅展示历史统计

### AC-6: 数据管理 UI
- **Given**: 用户打开数据管理页面
- **When**: 用户操作版本切换或缓存管理
- **Then**: UI 正常响应，数据正确加载或清理
- **Verification**: `human-judgment`

### AC-7: 胜率展示 UI
- **Given**: 用户打开胜率统计页面
- **When**: 页面加载完成
- **Then**: 清晰展示历史胜率统计，并有明显标识说明为历史数据，非实时预测
- **Verification**: `human-judgment`

### AC-8: 单元测试覆盖
- **Given**: 数据模块代码已实现
- **When**: 运行单元测试
- **Then**: 数据模块的核心功能测试通过，覆盖率达标
- **Verification**: `programmatic`

### AC-9: 合规性检查
- **Given**: 所有新功能已实现
- **When**: 进行合规审查
- **Then**: 确认胜率展示明确标明为历史统计，无实时预测功能
- **Verification**: `human-judgment`

## Open Questions
- [ ] Overwolf 事件 API 提供的数据完整性如何？是否能覆盖所有必要的游戏状态？
- [ ] 历史版本静态数据的存储策略是什么？本地文件系统还是数据库？
- [ ] 胜率统计的粒度如何？按阵容、按版本、按阶段？
- [ ] 数据缓存的过期策略是什么？
