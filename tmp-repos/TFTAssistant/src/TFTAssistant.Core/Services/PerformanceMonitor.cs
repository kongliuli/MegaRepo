using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;

namespace TFTAssistant.Core.Services;

/// <summary>
/// 性能监控服务
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly Dictionary<string, OperationStats> _operationStats = new();
    private readonly object _lock = new();
    private int _cacheHits = 0;
    private int _cacheMisses = 0;
    private int _totalRequests = 0;
    private readonly ILogger<PerformanceMonitor>? _logger;

    public PerformanceMonitor(ILogger<PerformanceMonitor>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 开始性能测量
    /// </summary>
    public IPerformanceMeasurement StartMeasurement(string operationName)
    {
        return new PerformanceMeasurement(this, operationName);
    }

    /// <summary>
    /// 获取性能统计数据
    /// </summary>
    public Task<PerformanceStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            var stats = new PerformanceStats
            {
                OperationStats = new Dictionary<string, OperationStats>(_operationStats),
                MemoryUsage = GetCurrentMemoryUsage(),
                CacheHitRate = CalculateCacheHitRate(),
                TotalRequests = _totalRequests,
                AverageResponseTime = CalculateAverageResponseTime()
            };

            return Task.FromResult(stats);
        }
    }

    /// <summary>
    /// 重置性能统计数据
    /// </summary>
    public Task ResetStatsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            _operationStats.Clear();
            _cacheHits = 0;
            _cacheMisses = 0;
            _totalRequests = 0;
            _logger?.LogInformation("Performance stats reset");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 记录操作执行时间
    /// </summary>
    internal void RecordOperationTime(string operationName, long elapsedMilliseconds)
    {
        lock (_lock)
        {
            if (!_operationStats.TryGetValue(operationName, out var stats))
            {
                stats = new OperationStats { OperationName = operationName };
                _operationStats[operationName] = stats;
            }

            stats.Count++;
            stats.TotalTime += elapsedMilliseconds;
            stats.MaxTime = Math.Max(stats.MaxTime, elapsedMilliseconds);
            stats.MinTime = Math.Min(stats.MinTime, elapsedMilliseconds);

            _totalRequests++;

            if (elapsedMilliseconds > 500)
            {
                _logger?.LogWarning("Slow operation: {OperationName} took {ElapsedMs}ms", operationName, elapsedMilliseconds);
            }
        }
    }

    /// <summary>
    /// 记录缓存命中
    /// </summary>
    public void RecordCacheHit()
    {
        Interlocked.Increment(ref _cacheHits);
    }

    /// <summary>
    /// 记录缓存未命中
    /// </summary>
    public void RecordCacheMiss()
    {
        Interlocked.Increment(ref _cacheMisses);
    }

    /// <summary>
    /// 计算缓存命中率
    /// </summary>
    private double CalculateCacheHitRate()
    {
        int total = _cacheHits + _cacheMisses;
        return total > 0 ? (double)_cacheHits / total : 0;
    }

    /// <summary>
    /// 计算平均响应时间
    /// </summary>
    private double CalculateAverageResponseTime()
    {
        long totalTime = _operationStats.Values.Sum(s => s.TotalTime);
        return _totalRequests > 0 ? (double)totalTime / _totalRequests : 0;
    }

    /// <summary>
    /// 获取当前内存使用情况
    /// </summary>
    private long GetCurrentMemoryUsage()
    {
        using var process = Process.GetCurrentProcess();
        return process.PrivateMemorySize64;
    }

    /// <summary>
    /// 性能测量实现
    /// </summary>
    private class PerformanceMeasurement : IPerformanceMeasurement
    {
        private readonly PerformanceMonitor _monitor;
        private readonly string _operationName;
        private readonly Stopwatch _stopwatch;
        private bool _stopped;

        public PerformanceMeasurement(PerformanceMonitor monitor, string operationName)
        {
            _monitor = monitor;
            _operationName = operationName;
            _stopwatch = Stopwatch.StartNew();
            _stopped = false;
        }

        /// <summary>
        /// 停止测量并记录结果
        /// </summary>
        public void Stop()
        {
            if (!_stopped)
            {
                _stopwatch.Stop();
                _monitor.RecordOperationTime(_operationName, _stopwatch.ElapsedMilliseconds);
                _stopped = true;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}