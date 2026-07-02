using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace TFTAssistant.Core.Services;

public sealed class ServiceManager : IDisposable
{
    private readonly ILogger<ServiceManager> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, IDisposable> _services;
    private bool _isRunning;
    
    public ServiceManager(ILogger<ServiceManager> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _services = new ConcurrentDictionary<string, IDisposable>();
        _isRunning = false;
    }
    
    public void RegisterService<T>(string serviceName, T service) where T : IDisposable
    {
        if (_services.TryAdd(serviceName, service))
        {
            _logger.LogInformation("服务已注册: {ServiceName}", serviceName);
        }
        else
        {
            _logger.LogWarning("服务注册失败: {ServiceName} (已存在)", serviceName);
        }
    }
    
    public T? GetService<T>(string serviceName) where T : IDisposable
    {
        if (_services.TryGetValue(serviceName, out var service) && service is T typedService)
        {
            return typedService;
        }
        return default;
    }
    
    public void StartAllServices()
    {
        if (_isRunning)
        {
            _logger.LogWarning("服务管理器已经在运行中");
            return;
        }
        
        _logger.LogInformation("开始启动所有服务");
        
        // 启动健康监控服务
        var healthService = _serviceProvider.GetService<HealthMonitoringService>();
        if (healthService != null)
        {
            healthService.Start();
            RegisterService("HealthMonitoring", healthService);
        }
        
        // 启动其他服务
        // 这里可以根据需要添加其他服务的启动逻辑
        
        _isRunning = true;
        _logger.LogInformation("所有服务启动完成");
    }
    
    public void StopAllServices()
    {
        if (!_isRunning)
        {
            _logger.LogWarning("服务管理器已经停止");
            return;
        }
        
        _logger.LogInformation("开始停止所有服务");
        
        // 停止健康监控服务
        var healthService = _serviceProvider.GetService<HealthMonitoringService>();
        if (healthService != null)
        {
            healthService.Stop();
        }
        
        // 停止其他服务
        // 这里可以根据需要添加其他服务的停止逻辑
        
        _isRunning = false;
        _logger.LogInformation("所有服务停止完成");
    }
    
    public void Dispose()
    {
        StopAllServices();
        
        _logger.LogInformation("开始释放所有服务");
        
        foreach (var service in _services.Values)
        {
            try
            {
                service.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "释放服务时发生错误");
            }
        }
        
        _services.Clear();
        _logger.LogInformation("所有服务释放完成");
    }
}