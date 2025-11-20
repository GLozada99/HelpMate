using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Startup;

public static class WebConfigs
{
    public static IServiceCollection ConfigureWeb(this IServiceCollection services)
    {
        return services;
    }

    public static WebApplication ConfigureWebApp(this WebApplication app)
    {
        return app;
    }

    public static WebApplication ConfigureSwagger(this WebApplication app)
    {
        return app;
    }

    public static IServiceCollection ConfigureApi(this IServiceCollection services)
    {
        return services;
    }
}
