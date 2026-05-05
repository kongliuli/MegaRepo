# 组件化需求规范

本文档定义了 practice-repository 项目整合过程中的组件化需求规范，用于指导项目的模块化、接口设计和依赖管理。

## 1. 模块化要求

### 1.1 模块划分原则

| 原则 | 说明 |
|------|------|
| **单一职责** | 每个模块只负责一个特定的功能领域 |
| **高内聚低耦合** | 模块内部紧密相关，模块间依赖最小化 |
| **可独立部署** | 模块可单独编译、测试和部署 |
| **接口抽象** | 通过接口定义模块边界，隐藏实现细节 |

### 1.2 模块类型定义

| 类型 | 说明 | 示例 |
|------|------|------|
| **Core** | 核心业务逻辑模块 | 领域模型、业务规则 |
| **Infrastructure** | 基础设施模块 | 数据访问、日志、配置 |
| **Presentation** | 表示层模块 | UI组件、API控制器 |
| **Shared** | 共享工具模块 | 工具类、扩展方法 |
| **Tests** | 测试模块 | 单元测试、集成测试 |

### 1.3 模块命名规范

```
[ProjectName].[ModuleType].[FeatureName]
```

**示例**:
- `UniversalInvoice.Core.InvoiceManagement`
- `InventoryPro.Infrastructure.DataAccess`
- `MegaRepo.Shared.Utilities`

### 1.4 模块结构模板

```
ModuleName/
├── src/
│   ├── ModuleName.csproj
│   ├── AssemblyInfo.cs
│   ├── PublicAPI/          # 公共接口定义
│   ├── Internal/           # 内部实现
│   └── Properties/
└── tests/
    └── ModuleName.Tests.csproj
```

---

## 2. 接口设计标准

### 2.1 接口命名规范

| 类型 | 命名规则 | 示例 |
|------|----------|------|
| 接口 | I + 名词 | `IInvoiceService` |
| 抽象类 | Abstract + 名词 | `AbstractRepository` |
| 实现类 | 名词 | `InvoiceService` |
| DTO | 名词 + Dto | `InvoiceDto` |
| 实体 | 名词 | `Invoice` |

### 2.2 接口设计原则

| 原则 | 说明 |
|------|------|
| **接口隔离** | 客户端不应依赖它不需要的接口 |
| **依赖倒置** | 高层模块不应依赖低层模块，两者都应依赖抽象 |
| **单一抽象** | 每个接口只定义一个角色或职责 |
| **稳定抽象** | 接口应相对稳定，避免频繁变更 |

### 2.3 API 设计规范

#### 2.3.1 RESTful API 命名

| 操作 | HTTP方法 | 路径示例 |
|------|----------|----------|
| 获取列表 | GET | `/api/invoices` |
| 获取单个 | GET | `/api/invoices/{id}` |
| 创建 | POST | `/api/invoices` |
| 更新 | PUT/PATCH | `/api/invoices/{id}` |
| 删除 | DELETE | `/api/invoices/{id}` |

#### 2.3.2 响应格式

```json
{
  "success": true,
  "data": {},
  "message": "",
  "error": null,
  "timestamp": "2025-01-07T10:30:00Z"
}
```

### 2.4 事件驱动接口

#### 2.4.1 事件命名规范

| 类型 | 命名规则 | 示例 |
|------|----------|------|
| 事件类 | 名词 + Event | `InvoiceCreatedEvent` |
| 事件处理器 | 名词 + Handler | `InvoiceCreatedHandler` |
| 事件发布器 | 名词 + Publisher | `DomainEventPublisher` |

---

## 3. 依赖管理规范

### 3.1 依赖类型分类

| 类型 | 说明 | 示例 |
|------|------|------|
| **框架依赖** | 语言/框架核心库 | .NET Core, Django |
| **基础设施依赖** | 第三方工具库 | EF Core, AutoMapper |
| **内部依赖** | 项目内部模块 | Core → Infrastructure |
| **测试依赖** | 测试框架和工具 | xUnit, Moq |

### 3.2 依赖注入规范

#### 3.2.1 生命周期管理

| 生命周期 | 适用场景 | 示例 |
|----------|----------|------|
| **Singleton** | 无状态服务、配置读取 | `IConfiguration`, `ILogger` |
| **Scoped** | 请求/会话级别服务 | `DbContext`, `IUnitOfWork` |
| **Transient** | 轻量级、有状态服务 | DTO 工厂、策略模式实现 |

#### 3.2.2 注册方式

```csharp
// Program.cs 或 Startup.cs
services.AddScoped<IInvoiceService, InvoiceService>();
services.AddSingleton<IConfiguration>(configuration);
services.AddTransient<IEmailSender, EmailSender>();
```

### 3.3 包管理规范

#### 3.3.1 版本控制策略

| 类型 | 版本策略 | 说明 |
|------|----------|------|
| 稳定版 | `x.y.z` | 正式发布版本 |
| 预发布版 | `x.y.z-preview.n` | 预览版本 |
| 开发版 | `x.y.z-dev.n` | 开发中版本 |

#### 3.3.2 依赖声明

- **NuGet**: 使用 `PackageReference` 声明
- **Python**: 使用 `requirements.txt` 或 `pyproject.toml`
- **锁定文件**: 提交 `packages.lock.json` 或 `poetry.lock`

### 3.4 循环依赖检测

- 使用 IDE 工具检测项目间循环依赖
- 建立依赖关系图文档
- 定期审查依赖变更

---

## 4. 代码规范

### 4.1 命名规范

| 类型 | 规则 | 示例 |
|------|------|------|
| 命名空间 | PascalCase | `MegaRepo.Core.Invoices` |
| 类/接口 | PascalCase | `InvoiceService`, `IInvoiceRepository` |
| 方法 | PascalCase | `CreateInvoice()`, `GetById()` |
| 属性 | PascalCase | `InvoiceNumber`, `TotalAmount` |
| 字段 | camelCase | `_invoiceRepository`, `_logger` |
| 参数 | camelCase | `invoiceDto`, `cancellationToken` |
| 局部变量 | camelCase | `result`, `filteredList` |
| 常量 | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT`, `DEFAULT_TIMEOUT` |

### 4.2 代码风格

- **缩进**: 4 个空格
- **换行**: Unix LF (`\n`)
- **大括号**: Allman 风格（单独一行）
- **空格**: 运算符前后各一个空格

### 4.3 注释规范

| 类型 | 规则 |
|------|------|
| 类/接口 | 必须有 XML 文档注释 |
| 公共方法 | 必须有 XML 文档注释 |
| 私有方法 | 复杂逻辑应有注释说明 |
| 复杂算法 | 应有注释解释设计思路 |
| 代码警告 | 使用 `// TODO:` 或 `// FIXME:` 标记 |

---

## 5. 配置管理规范

### 5.1 配置文件结构

```
config/
├── appsettings.json          # 主配置文件
├── appsettings.Development.json  # 开发环境配置
├── appsettings.Production.json   # 生产环境配置
└── secrets/                  # 敏感配置（git 忽略）
```

### 5.2 配置分类

| 类别 | 示例 |
|------|------|
| **数据库** | 连接字符串、超时设置 |
| **外部服务** | API 密钥、端点地址 |
| **应用设置** | 功能开关、默认值 |
| **日志配置** | 级别、输出目标 |

### 5.3 敏感信息处理

- 敏感配置不应提交到版本控制
- 使用环境变量或密钥管理服务
- 配置文件中使用占位符

---

## 6. 测试规范

### 6.1 测试类型

| 类型 | 说明 | 覆盖范围 |
|------|------|----------|
| **单元测试** | 测试单个组件 | 类、方法 |
| **集成测试** | 测试组件协作 | 模块间交互 |
| **端到端测试** | 测试完整流程 | 用户场景 |

### 6.2 测试命名规范

```
[MethodName]_[Scenario]_[ExpectedResult]
```

**示例**:
- `CreateInvoice_WithValidData_ReturnsInvoice`
- `GetInvoice_WithInvalidId_ThrowsNotFoundException`

### 6.3 测试覆盖率目标

| 模块类型 | 覆盖率目标 |
|----------|----------|
| Core 模块 | ≥ 80% |
| Infrastructure 模块 | ≥ 60% |
| Presentation 模块 | ≥ 40% |

---

## 附录：组件化检查清单

### 模块划分
- [ ] 每个模块职责单一
- [ ] 模块间依赖最小化
- [ ] 模块可独立编译和测试

### 接口设计
- [ ] 接口符合单一职责原则
- [ ] 使用接口隔离依赖
- [ ] API 设计符合 RESTful 规范

### 依赖管理
- [ ] 依赖关系清晰
- [ ] 无循环依赖
- [ ] 使用依赖注入

### 代码质量
- [ ] 命名规范一致
- [ ] 代码注释完整
- [ ] 测试覆盖率达标

---

*文档版本：1.0*
*适用范围：MegaRepo 项目整合*
