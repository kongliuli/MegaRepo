using System.Timers;
using Microsoft.Extensions.Logging;

namespace TFTAssistant.Core.Services;

public sealed class HealthMonitoringService : IDisposable
{
    private readonly ILogger<HealthMonitoringService> _logger;
    private readonly Timer _healthCheckTimer;
    private readonly Timer _memoryCheckTimer;
    private int _consecutiveErrors = 0;
    private const int MaxConsecutiveErrors = 10;
    private bool _isHealthy = true;
    
    public event EventHandler<HealthStatusChangedEventArgs>? HealthStatusChanged;
    
    public HealthMonitoringService(ILogger<HealthMonitoringService> logger)
    {
        _logger = logger;
        
        // 健康检查定时器，每30秒执行一次
        _healthCheckTimer = new Timer(30000);
        _healthCheckTimer.Elapsed += OnHealthCheckElapsed;
        _healthCheckTimer.AutoReset = true;
        
        // 内存检查定时器，每5分钟执行一次
        _memoryCheckTimer = new Timer(300000);
        _memoryCheckTimer.Elapsed += OnMemoryCheckElapsed;
        _memoryCheckTimer.AutoReset = true;
    }
    
    public void Start()
    {
        _healthCheckTimer.Start();
        _memoryCheckTimer.Start();
        _logger.LogInformation("健康监控服务已启动");
    }
    
    public void Stop()
    {
        _healthCheckTimer.Stop();
        _memoryCheckTimer.Stop();
        _logger.LogInformation("健康监控服务已停止");
    }
    
    public void ReportError(string errorMessage, Exception? exception = null)
    {
        _consecutiveErrors++;
        _logger.LogWarning("系统错误: {ErrorMessage}, 连续错误数: {ErrorCount}", errorMessage, _consecutiveErrors);
        
        if (exception != null)
        {
            _logger.LogError(exception, "系统错误详情");
        }
        
        CheckHealthStatus();
    }
    
    public void ReportSuccess()
    {
        if (_consecutiveErrors > 0)
        {
            _consecutiveErrors = 0;
            _logger.LogInformation("系统恢复正常");
            CheckHealthStatus();
        }
    }
    
    private void OnHealthCheckElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            // 执行健康检查
            var status = CheckSystemHealth();
            
            if (status != _isHealthy)
            {
                _isHealthy = status;
                HealthStatusChanged?.Invoke(this, new HealthStatusChangedEventArgs(_isHealthy));
                
                if (_isHealthy)
                {
                    _logger.LogInformation("系统健康状态: 正常");
                }
                else
                {
                    _logger.LogWarning("系统健康状态: 异常");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "健康检查执行失败");
        }
    }
    
    private void OnMemoryCheckElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            CheckMemoryUsage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "内存检查执行失败");
        }
    }
    
    private bool CheckSystemHealth()
    {
        // 检查连续错误数
        if (_consecutiveErrors >= MaxConsecutiveErrors)
        {
            return false;
        }
        
        // 检查系统资源
        var memoryStatus = CheckMemoryStatus();
        if (!memoryStatus)
        {
            return false;
        }
        
        // 检查其他系统指标
        // TODO: 添加更多健康检查逻辑
        
        return true;
    }
    
    private bool CheckMemoryStatus()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryMB = process.WorkingSet64 / (1024 * 1024);
        
        // 检查内存使用情况，超过1GB时发出警告
        if (memoryMB > 1024)
        {
            _logger.LogWarning("内存使用过高: {MemoryMB}MB", memoryMB);
            return false;
        }
        
        _logger.LogDebug("内存使用正常: {MemoryMB}MB", memoryMB);
        return true;
    }
    
    private void CheckMemoryUsage()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryMB = process.WorkingSet64 / (1024 * 1024);
        var cpuUsage = GetCpuUsage();
        
        _logger.LogInformation("系统资源使用情况: 内存={MemoryMB}MB, CPU={CpuUsage}%", memoryMB, cpuUsage);
    }
    
    private float GetCpuUsage()
    {
        // 简单的CPU使用率计算
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var startTime = DateTime.Now;
        var startCpuUsage = process.TotalProcessorTime;
        
        System.Threading.Thread.Sleep(100);
        
        var endTime = DateTime.Now;
        var endCpuUsage = process.TotalProcessorTime;
        
        var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
        var totalMsPassed = (endTime - startTime).TotalMilliseconds;
        var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;
        
        return (float)Math.Round(cpuUsageTotal, 2);
    }
    
    private void CheckHealthStatus()
    {
        var newStatus = CheckSystemHealth();
        if (newStatus != _isHealthy)
        {
            _isHealthy = newStatus;
            HealthStatusChanged?.Invoke(this, new HealthStatusChangedEventArgs(_isHealthy));
        }
    }
    
    public void Dispose()
    {
        _healthCheckTimer.Dispose();
        _memoryCheckTimer.Dispose();
    }
}

public class HealthStatusChangedEventArgs : EventArgs
{
    public bool IsHealthy { get; }
    
    public HealthStatusChangedEventArgs(bool isHealthy)
    {
        IsHealthy = isHealthy;
    }
}