using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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


    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfigurationManager configuration)
    {
        services.AddDbContext<HelpMateDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }
}
