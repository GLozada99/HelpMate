using API.Helpers.Response;
using API.Interfaces.Response;
using API.Services.Tracking;
using Application.Interfaces.Auth;
using Application.Interfaces.Tracking;
using Infrastructure.Context;
using Infrastructure.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Startup;

public static class CommonConfigs
{
    public static IServiceCollection ConfigureDependencies(
        this IServiceCollection services)
    {
        services.AddScoped<ITrackingIdProvider, TrackingIdProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
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
