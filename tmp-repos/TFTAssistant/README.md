# TFTAssistant

TFTAssistant 是一个为 Teamfight Tactics（云顶之弈）玩家设计的辅助工具，旨在提升游戏体验和竞技水平。

## 功能特性

### 已实现功能
- **核心架构**：完整的分层架构设计（抽象接口、数据层、引擎层、服务层）
- **数据模型**：全面的游戏数据模型定义，包括玩家、游戏状态、单位等
- **智能推荐**：装备合成、阵容匹配、经济管理和强化符文推荐算法
- **数据存储**：SQLite 本地数据库实现，支持游戏状态和匹配记录存储
- **前端界面**：桌面应用和游戏内覆盖层基础结构
- **测试框架**：完整的单元测试体系，确保功能稳定性
- **日志系统**：结构化日志记录，便于问题排查

### 待实现功能
- **实时游戏数据**：与 Overwolf 集成，实时获取游戏状态
- **数据分析**：个人游戏数据统计、胜率分析和趋势图
- **用户体验**：响应式设计、多语言支持和主题切换
- **大数据集成**：外部数据源接入和离线数据支持

## 安装指南

### 系统要求
- Windows 10/11 或 macOS 10.15+
- 至少 4GB 内存
- 稳定的网络连接

### 安装步骤
1. 从 [GitHub Releases](https://github.com/yourusername/TFTAssistant/releases) 下载最新版本
2. 运行安装程序并按照提示完成安装
3. 启动应用程序并登录您的游戏账号

## 使用方法

1. **启动应用**：打开 TFTAssistant 应用程序
2. **连接游戏**：确保游戏正在运行，应用会自动检测并连接
3. **查看推荐**：在游戏过程中，应用会实时显示阵容和装备推荐
4. **分析数据**：在游戏结束后，查看详细的数据分析报告

## 开发说明

### 技术栈
- 前端：原生 HTML + CSS + JavaScript
- 后端：C# .NET Core
- 数据存储：SQLite
- 游戏集成：Overwolf API

### 代码规范
请参考 [代码规范文档](CODE_STYLE.md) 了解项目的代码风格和规范要求。

### 贡献指南
1. Fork 本仓库
2. 创建您的特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交您的更改 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开一个 Pull Request

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 联系方式

- 作者：化自在
- 邮箱：your.email@example.com
- GitHub：[yourusername](https://github.com/yourusername)

---

感谢使用 TFTAssistant！祝您游戏愉快！