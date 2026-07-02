using System.Threading;
using System.Threading.Tasks;

namespace TFTAssistant.Core.Abstractions;

/// <summary>
/// 性能监控接口
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// 开始性能测量
    /// </summary>
    /// <param name="operationName">操作名称</param>
    /// <returns>性能测量令牌</returns>
    IPerformanceMeasurement StartMeasurement(string operationName);
    
    /// <summary>
    /// 获取性能统计数据
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>性能统计数据</returns>
    Task<PerformanceStats> GetStatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 重置性能统计数据
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task ResetStatsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// 性能测量接口
/// </summary>
public interface IPerformanceMeasurement : System.IDisposable
{
    /// <summary>
    /// 停止测量并记录结果
    /// </summary>
    void Stop();
}

/// <summary>
/// 性能统计数据
/// </summary>
public class PerformanceStats
{
    /// <summary>
    /// 操作统计信息
    /// </summary>
    public Dictionary<string, OperationStats> OperationStats { get; set; } = new();
    
    /// <summary>
    /// 内存使用情况（字节）
    /// </summary>
    public long MemoryUsage { get; set; }
    
    /// <summary>
    /// 缓存命中率
    /// </summary>
    public double CacheHitRate { get; set; }
    
    /// <summary>
    /// 总请求数
    /// </summary>
    public int TotalRequests { get; set; }
    
    /// <summary>
    /// 平均响应时间（毫秒）
    /// </summary>
    public double AverageResponseTime { get; set; }
}

/// <summary>
/// 操作统计信息
/// </summary>
public class OperationStats
{
    /// <summary>
    /// 操作名称
    /// </summary>
    public string OperationName { get; set; }
    
    /// <summary>
    /// 执行次数
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// 总执行时间（毫秒）
    /// </summary>
    public long TotalTime { get; set; }
    
    /// <summary>
    /// 平均执行时间（毫秒）
    /// </summary>
    public double AverageTime => Count > 0 ? (double)TotalTime / Count : 0;
    
    /// <summary>
    /// 最大执行时间（毫秒）
    /// </summary>
    public long MaxTime { get; set; }
    
    /// <summary>
    /// 最小执行时间（毫秒）
    /// </summary>
    public long MinTime { get; set; } = long.MaxValue;
}