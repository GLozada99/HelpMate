using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Startup;

public static class CommonConfigs
{
    public static IServiceCollection ConfigureDependencies(
        this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection ConfigureSettings(this IServiceCollection services)
    {
        return services;
    }


    public static IServiceCollection ConfigureDatabase(this IServiceCollection services)
    {
        return services;
    }
}
