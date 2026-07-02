# vibe-coding-platform 删除前复活说明

日期：2026-07-02
仓库：`D:\Code\githubDown\vibe-coding-platform`

## 结论

建议留档后删除本地仓库。

这是 Vercel 风格的 AI coding platform 模板，不像你的长期业务主线。价值在产品结构和少量实现路径，不需要长期保留整个仓。

## 项目定位

用户输入 prompt，AI 在 sandbox 中生成应用，并提供实时预览、文件浏览器、命令日志、错误监控和一键部署。

## 可复用点

| 路径 | 价值 |
| --- | --- |
| `app/api/chat/route.ts` | prompt 到 agent 流程入口 |
| `ai/tools/create-sandbox.ts` | 创建 Vercel Sandbox |
| `ai/tools/generate-files.ts` | 生成文件工具 |
| `ai/tools/run-command.ts` | sandbox 命令执行 |
| `app/api/sandboxes/[sandboxId]` | sandbox 文件、命令、日志 API |
| `components/file-explorer` | 文件树和文件内容浏览 |
| `components/commands-logs` | 命令日志流式展示 |
| `components/preview/preview.tsx` | 实时预览区域 |

## 不保留原因

- 模板属性强，README 直接指向 Vercel 示例。
- 依赖 Vercel AI Gateway / Sandbox，长期运行成本和平台绑定都高。
- 与当前活跃项目没有直接业务关系。

## 复活路径

后续如果要做“AI 生成应用 + sandbox 预览”，优先恢复上述路径，不要整体复活仓库。

