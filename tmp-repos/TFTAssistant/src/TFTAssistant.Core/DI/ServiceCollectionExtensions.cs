using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TFTAssistant.Core.Abstractions;
using TFTAssistant.Core.Data;
using TFTAssistant.Core.Engine;
using TFTAssistant.Core.Logging;
using TFTAssistant.Core.Services;

namespace TFTAssistant.Core.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTFTAssistantCore(this IServiceCollection services)
    {
        // 日志系统
        services.AddSingleton<ILoggerFactory>(provider =>
        {
            LogManager.Initialize();
            return new SerilogLoggerFactory();
        });
        
        services.AddSingleton(typeof(ILogger<>), typeof(SerilogLogger<>));
        
        // 数据提供者
        services.AddSingleton<IGameDataProvider, LiveClientDataProvider>();
        services.AddSingleton<IStaticDataProvider, DataDragonProvider>();
        services.AddSingleton<CompositeGameDataProvider>();
        
        // 仓库
        services.AddSingleton<IGameStateRepository, SqliteGameStateRepository>();
        services.AddSingleton<IMatchRepository, SqliteMatchRepository>();
        
        // 引擎
        services.AddSingleton<ICompMatcher, CompMatcher>();
        services.AddSingleton<IItemAdvisor, ItemAdvisor>();
        services.AddSingleton<IEconomyAdvisor, EconomyAdvisor>();
        services.AddSingleton<IAugmentAdvisor, AugmentAdvisor>();
        services.AddSingleton<IRecommendationEngine, RecommendationEngine>();
        
        // 服务
        services.AddSingleton<HealthMonitoringService>();
        services.AddSingleton<ServiceManager>();
        services.AddSingleton<IChampionAnalysisService, ChampionAnalysisService>();
        services.AddSingleton<ILineupAnalysisService, LineupAnalysisService>();
        services.AddSingleton<IMetaAnalysisService, MetaAnalysisService>();
        services.AddSingleton<IWinRateStatisticsService, WinRateStatisticsService>();
        services.AddSingleton<IEquipmentManager, EquipmentManager>();
        services.AddSingleton<EquipmentDataService>();
        services.AddSingleton<EquipmentCalculationService>();
        services.AddSingleton<EquipmentRecommendationService>();
        
        // 缓存
        services.AddMemoryCache();
        services.AddSingleton<IBigDataCache>(provider =>
        {
            var logger = provider.GetService<ILogger<FileSystemBigDataCache>>();
            var cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TFTAssistant", "Cache");
            return new FileSystemBigDataCache(cacheDirectory, 200, TimeSpan.FromDays(7), logger);
        });
        
        // 外部数据源
        services.AddSingleton<IGameDataSource>(provider =>
        {
            var httpClient = provider.GetService<HttpClient>();
            var logger = provider.GetService<ILogger<ExternalApiDataSource>>();
            // 这里使用配置的API地址和密钥，实际应用中应该从配置文件读取
            var apiBaseUrl = "https://api.tftassistant.com/v1";
            var apiKey = "your-api-key";
            return new ExternalApiDataSource(httpClient!, apiBaseUrl, apiKey, logger);
        });
        
        // 大数据服务
        services.AddSingleton<BigDataRepository>();
        services.AddSingleton<DataVersionManager>();
        services.AddSingleton<IDataVersionManager>(provider => provider.GetService<DataVersionManager>()!);
        services.AddSingleton<IBigDataService>(provider =>
        {
            var repository = provider.GetService<BigDataRepository>();
            var cache = provider.GetService<IBigDataCache>();
            var versionManager = provider.GetService<IDataVersionManager>();
            var externalDataSource = provider.GetService<IGameDataSource>();
            var logger = provider.GetService<ILogger<BigDataService>>();
            return new BigDataService(repository!, cache!, versionManager!, externalDataSource!, logger);
        });
        
        // HTTP 客户端
        services.AddHttpClient();
        
        return services;
    }
    
    private class SerilogLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
            // 不需要添加其他提供者
        }
        
        public ILogger CreateLogger(string categoryName)
        {
            return LogManager.CreateLogger<object>();
        }
        
        public void Dispose()
        {
            // 不需要释放资源
        }
    }
    
    private class SerilogLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;
        
        public SerilogLogger(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger(typeof(T).FullName!);
        }
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
        
        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }
        
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }
    }
}