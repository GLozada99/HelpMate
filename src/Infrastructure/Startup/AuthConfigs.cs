using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Startup;

public static class AuthConfigs
{
    public static IServiceCollection ConfigureCookies(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection ConfigureCoors(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection ConfigurePolicies(this IServiceCollection services)
    {
        return services;
    }
}
