# VibeSkills 复活说明

日期：2026-07-02
仓库：`D:\Code\githubDown\VibeSkills`
结论：计划删除；保留本文档用于未来复活。

## 项目定位

`VibeSkills` 是一个 Trae/Codex 风格的技能文档仓，内容几乎全是 `SKILL.md` 和 README。

它不是运行时代码仓，而是提示词/工作流知识库。

## 当前技能分类

| 分类 | 示例 |
| --- | --- |
| 通用 Skills | `test-generator`、`project-architect`、`git-workflow-assistant`、`documentation-engineer`、`code-style-reviewer` |
| C# WinForms | `event-handler`、`data-binding-expert`、`control-layout`、`dotnet-migration-assistant` |
| C# Avalonia | `window-manager`、`Avalonia-UI-Optimizer`、`cross-platform-packager`、`avalonia-stylist` |
| C# Blazor | `blazor-component-generator`、`blazor-router-configurator`、`state-manager`、`js-interop-expert` |
| C# common | `config-manager`、`di-config-expert`、`exception-handler`、`nuget-manager`、`log-architect`、`interface-designer` |
| C# webapi-mvc | `swagger-configurator`、`filter-expert`、`middleware-developer`、`api-controller-generator` |
| Python | `virtual-env-manager`、`type-hint-expert`、`pypi-publisher`、`fastapi-generator` |
| JavaScript | `component-scaffold`、`form-validator`、`api-call-generator` |
| ProjectSkills/xinglin | `layout-expert`、`json-template-expert` |

## 可复用点

如果未来要复活，不需要整仓恢复，只取这些方向：

- `ProjectSkills/xinglin/*`：和活跃项目 `xinglin` 相关。
- `C#/Avalonia/*`：如果继续做 Avalonia 桌面应用。
- `C#/blazor/*`：如果继续做 Blazor 生成/路由/状态管理。
- `通用Skills/project-architect`、`documentation-engineer`：可作为文档/架构提示词素材。

## 不保留原因

1. 当前 Codex 已有正式 skill/plugin 体系，旧仓容易重复。
2. 内容多是提示词文档，不是不可替代代码。
3. 大部分 skills 没有安装/验证流程。
4. 后续维护成本高，收益不如按需把少数 skill 移入正式 Codex skills。

## 复活路径

如果后续需要某个 skill：

1. 从本文档定位分类。
2. 只恢复对应 `SKILL.md`。
3. 按当前 Codex skill 规范重建，不整仓恢复。
