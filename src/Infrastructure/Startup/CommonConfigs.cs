using API.Helpers.Response;
using API.Interfaces.Response;
using Application.Interfaces.Tracking;
using Infrastructure.Context;
using Infrastructure.Helpers.Tracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Startup;

public static class CommonConfigs
{
    public static IServiceCollection ConfigureDependencies(
        this IServiceCollection services)
    {
        services.AddScoped<ITrackingIdHelper, TrackingIdHelper>();
        services.AddScoped<IApiResponseHelper, ApiResponseHelper>();
        return services;
    }

    public static IServiceCollection ConfigureSettings(this IServiceCollection services)
    {
        return services;
    }


    public static IServiceCollection ConfigureDatabase(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        services.AddDbContext<HelpMateDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }
}
