# 代码规范文档

## 1. 命名规范

### 1.1 类和接口命名
- 使用 PascalCase 命名法
- 类名使用名词或名词短语
- 接口名以 `I` 前缀开头，后跟 PascalCase 命名

**示例**：
```csharp
public class RecommendationEngine { }
public interface ICompMatcher { }
```

### 1.2 方法命名
- 使用 PascalCase 命名法
- 方法名使用动词或动词短语

**示例**：
```csharp
public void CalculateEconomy() { }
public async Task<RecommendationSet> GetRecommendationsAsync() { }
```

### 1.3 变量和字段命名
- 局部变量和参数使用 camelCase 命名法
- 私有字段使用 `_` 前缀加 camelCase 命名法
- 公共属性使用 PascalCase 命名法

**示例**：
```csharp
private readonly ICompMatcher _compMatcher;
public int TotalGold { get; set; }
```

### 1.4 常量命名
- 使用 PascalCase 命名法
- 全大写，单词间用下划线分隔

**示例**：
```csharp
public const int MAX_LEVEL = 9;
public const string DEFAULT_CACHE_KEY = "default";
```

## 2. 代码风格

### 2.1 缩进和空格
- 使用 4 个空格进行缩进
- 大括号 `{}` 单独占一行
- 运算符两侧添加空格
- 逗号后添加空格

**示例**：
```csharp
if (condition)
{
    // 代码
}

var result = a + b;
var list = new List<string> { "a", "b", "c" };
```

### 2.2 行长度
- 每行代码长度不超过 120 个字符
- 超过长度的代码应适当换行

### 2.3 空行
- 方法之间添加空行
- 逻辑块之间添加空行
- 类成员之间添加空行

## 3. 注释规范

### 3.1 类和接口注释
- 使用 XML 文档注释
- 描述类或接口的功能和用途
- 包含主要职责和使用场景

**示例**：
```csharp
/// <summary>
/// 推荐引擎
/// 整合各种建议功能，提供综合推荐
/// </summary>
public sealed class RecommendationEngine : IRecommendationEngine
```

### 3.2 方法注释
- 使用 XML 文档注释
- 描述方法的功能、参数和返回值
- 包含异常信息（如果有）

**示例**：
```csharp
/// <summary>
/// 获取游戏推荐
/// </summary>
/// <param name="state">游戏状态</param>
/// <param name="cancellationToken">取消令牌</param>
/// <returns>推荐结果集</returns>
public async Task<RecommendationSet> GetRecommendationsAsync(GameState state, CancellationToken cancellationToken = default)
```

### 3.3 代码注释
- 对复杂的业务逻辑添加注释
- 解释关键算法和决策过程
- 避免冗余注释（如简单的赋值操作）

**示例**：
```csharp
// 生成唯一的缓存键，基于游戏状态的关键信息
var keyBuilder = new System.Text.StringBuilder();
```

## 4. 异常处理

### 4.1 异常捕获
- 只捕获必要的异常
- 避免捕获通用异常（如 `Exception`）
- 对捕获的异常进行适当处理

**示例**：
```csharp
try
{
    // 代码
}
catch (OperationCanceledException)
{
    // 处理取消操作
}
catch (SpecificException ex)
{
    // 处理特定异常
    _logger.LogError(ex, "操作失败");
}
```

### 4.2 异常日志
- 使用结构化日志记录异常
- 包含上下文信息
- 避免在日志中包含敏感信息

## 5. 异步编程

### 5.1 异步方法命名
- 异步方法名以 `Async` 后缀结尾

**示例**：
```csharp
public async Task<RecommendationSet> GetRecommendationsAsync() { }
```

### 5.2 取消令牌
- 为长时间运行的异步操作提供取消令牌
- 正确处理取消操作

**示例**：
```csharp
public async Task<RecommendationSet> GetRecommendationsAsync(GameState state, CancellationToken cancellationToken = default)
```

## 6. 性能优化

### 6.1 缓存
- 对频繁访问的数据使用缓存
- 合理设置缓存过期时间

### 6.2 并行处理
- 对独立的操作使用并行处理
- 避免过度并行导致的性能问题

**示例**：
```csharp
var tasks = new List<Task>
{
    Task1(),
    Task2(),
    Task3()
};

await Task.WhenAll(tasks);
```

## 7. 测试规范

### 7.1 测试命名
- 测试类名以 `Tests` 后缀结尾
- 测试方法名使用 `MethodName_Scenario_ExpectedResult` 格式

**示例**：
```csharp
public class RecommendationEngineTests
{
    [Fact]
    public void GetRecommendationsAsync_ValidState_ReturnsRecommendations() { }
}
```

### 7.2 测试覆盖
- 确保关键功能有测试覆盖
- 测试边界条件和异常情况

## 8. 版本控制

### 8.1 提交信息
- 提交信息应清晰明了
- 遵循 `类型: 描述` 格式
- 类型包括：feat（新功能）、fix（修复）、docs（文档）、style（代码风格）、refactor（重构）、test（测试）、chore（构建/依赖）

**示例**：
```
feat: 添加装备推荐功能
fix: 修复经济计算错误
docs: 更新项目文档
```

### 8.2 分支管理
- 主分支：main
- 开发分支：develop
- 特性分支：feature/xxx
- 修复分支：fix/xxx

## 9. 工具和配置

### 9.1 代码分析工具
- 使用 .NET 代码分析器
- 遵循 IDE 建议的代码改进

### 9.2 格式化工具
- 使用 .NET 格式化工具
- 保持代码风格一致

## 10. 最佳实践

- 遵循 SOLID 原则
- 优先使用依赖注入
- 避免魔法数字和硬编码值
- 使用枚举替代字符串常量
- 保持方法职责单一
- 合理使用设计模式

---

本规范适用于 TFTAssistant 项目的所有代码文件，所有开发者应严格遵守。