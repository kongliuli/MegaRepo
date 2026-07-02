using System.IO;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace TFTAssistant.Core.Logging;

public static class LogManager
{
    private static bool _isInitialized = false;
    
    public static void Initialize()
    {
        if (_isInitialized)
            return;
        
        var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                Path.Combine(logDirectory, "tft-assistant-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {SourceContext} - {Message:lj} {Properties:j}{NewLine}{Exception}");
        
        Log.Logger = loggerConfiguration.CreateLogger();
        _isInitialized = true;
        
        Log.Information("日志系统初始化完成");
    }
    
    public static ILogger<T> CreateLogger<T>()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
        
        return new SerilogLogger<T>(Log.Logger);
    }
    
    private class SerilogLogger<T> : ILogger<T>
    {
        private readonly ILogger _logger;
        
        public SerilogLogger(ILogger logger)
        {
            _logger = logger.ForContext<T>();
        }
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var serilogLevel = ConvertLogLevel(logLevel);
            _logger.Write(serilogLevel, exception, formatter(state, exception));
        }
        
        public bool IsEnabled(LogLevel logLevel)
        {
            var serilogLevel = ConvertLogLevel(logLevel);
            return _logger.IsEnabled(serilogLevel);
        }
        
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }
        
        private static LogEventLevel ConvertLogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => LogEventLevel.Information
            };
        }
    }
}