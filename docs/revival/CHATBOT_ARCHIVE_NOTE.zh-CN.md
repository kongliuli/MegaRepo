# chatbot 删除前复活说明

日期：2026-07-02
仓库：`D:\Code\githubDown\chatbot`

## 结论

建议留档后删除本地仓库。

这是 Vercel AI SDK Chatbot 模板，价值主要是上游模板结构：Auth、数据库、文件上传、artifact、测试和 AI provider 组织方式。

## 项目定位

Next.js + AI SDK 的完整聊天应用模板，包含：

- 登录/注册/访客认证。
- 聊天历史、消息、投票、文件上传。
- 文本、代码、图片、表格等 artifact。
- Drizzle/Postgres 数据层。
- Playwright e2e 测试。

## 可复用点

| 路径 | 价值 |
| --- | --- |
| `app/(chat)/api/chat/route.ts` | 聊天 API 主入口 |
| `lib/ai/providers.ts` | 多模型 provider 组织 |
| `lib/ai/models.ts` | 模型配置 |
| `lib/db/schema.ts` | 聊天应用数据库结构 |
| `lib/db/queries.ts` | 聊天历史和消息查询 |
| `artifacts` | artifact server/client 结构 |
| `components/chat` | 聊天 UI 组件集合 |
| `tests/e2e` | 可参考的端到端测试结构 |

## 不保留原因

- 明确是模板仓，不是你的业务产品。
- 依赖多，维护面大。
- 如果需要 chatbot 能力，直接参考 AI SDK/Vercel 官方模板更划算。

## 复活路径

后续要做聊天产品时，按模块拿：

1. 最先看 `lib/ai/providers.ts` 和 `app/(chat)/api/chat/route.ts`。
2. 需要持久化再看 `lib/db/schema.ts` / `queries.ts`。
3. 需要多模态编辑再看 `artifacts`。

