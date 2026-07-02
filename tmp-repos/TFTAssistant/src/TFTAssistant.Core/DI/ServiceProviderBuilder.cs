using Microsoft.Extensions.DependencyInjection;

namespace TFTAssistant.Core.DI;

public static class ServiceProviderBuilder
{
    public static ServiceProvider Build()
    {
        var services = new ServiceCollection();
        services.AddTFTAssistantCore();
        return services.BuildServiceProvider();
    }
    
    public static IServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddTFTAssistantCore();
        return services;
    }
}