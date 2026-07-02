using Microsoft.Extensions.DependencyInjection;
using TFTAssistant.Core.DI;
using TFTAssistant.Core.Services;

// 构建依赖注入容器
var serviceProvider = ServiceProviderBuilder.Build();

// 获取服务管理器并启动所有服务
var serviceManager = serviceProvider.GetRequiredService<ServiceManager>();
serviceManager.StartAllServices();

Console.WriteLine("TFT Assistant 服务已启动");

// 保持应用程序运行
Console.WriteLine("按任意键退出...");
Console.ReadKey();

// 停止所有服务
serviceManager.StopAllServices();
Console.WriteLine("TFT Assistant 服务已停止");
