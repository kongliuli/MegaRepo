# FileExplorer

## 项目概述

FileExplorer是一个基于WPF的现代化文件浏览器应用，采用MVVM架构设计，提供直观的文件浏览和管理功能。

## 功能特性

- 现代化的文件浏览器界面
- 支持文件和文件夹的浏览、创建、删除、重命名等操作
- 采用MVVM架构，代码结构清晰
- 响应式设计，适配不同屏幕尺寸
- 使用了MahApps.Metro和Material.Icons等现代UI库

## 技术栈

- .NET 8.0
- WPF
- MVVM架构
- MahApps.Metro（UI框架）
- Material.Icons（图标库）

## 项目结构

```
FileExplorer/
├── View/                  # 视图层
│   ├── MainContentControl.xaml  # 主内容控件
│   └── MainContentControl.xaml.cs
├── ViewModel/             # 视图模型层
│   ├── MainViewModel.cs   # 主视图模型
│   └── RelayCommand.cs    # 命令实现
├── App.xaml               # 应用入口
├── MainWindow.xaml        # 主窗口
└── FileExplorer.csproj    # 项目文件
```

## 主要功能

- 文件和文件夹的浏览
- 文件和文件夹的创建、删除、重命名
- 文件属性查看
- 搜索功能
- 快捷操作

## 运行说明

1. 打开解决方案文件 `FileExplorer.sln`
2. 设置 `FileExplorer` 为启动项目
3. 运行项目
4. 使用界面浏览和管理文件系统

## 开发说明

- 采用了MVVM架构，实现了视图和逻辑的分离
- 使用了RelayCommand实现命令绑定
- 支持通过配置扩展功能
- 可自定义主题和样式

## 许可证

MIT