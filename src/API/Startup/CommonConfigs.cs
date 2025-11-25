using API.Helpers.Response;
using API.Interfaces.Response;
using API.Services.Tracking;
using Application.Interfaces.Auth;
using Application.Interfaces.Board;
using Application.Interfaces.Ticket;
using Application.Interfaces.Tracking;
using Application.Interfaces.User;
using Domain.Entities.User;
using Domain.Enums;
using Infrastructure.Context;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Board;
using Infrastructure.Services.Ticket;
using Infrastructure.Services.User;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Startup;

public static class CommonConfigs
{
    public static IServiceCollection ConfigureDependencies(
        this IServiceCollection services)
    {
        services.AddSingleton<ITrackingIdProvider, TrackingIdProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IApiResponseHelper, ApiResponseHelper>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

    public static IServiceCollection ConfigureSettings(this IServiceCollection services)
    {
        return services;
    }


    public static IServiceCollection ConfigureDatabase(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        var seedingData = configuration.GetSection("Seeding");
        var email = seedingData.GetValue<string>("Email");
        var password = seedingData.GetValue<string>("Password");
        var fullName = seedingData.GetValue<string>("FullName");

        services.AddDbContext<HelpMateDbContext>((sp, options) =>
            {
                var passwordHasher = sp.GetRequiredService<IPasswordHasher>();

                options.UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection"))
                    .UseSeeding((context, _) =>
                    {
                        if (context.Set<User>().Any())
                            return;

                        if (email is null || password is null ||
                            fullName is null)
                            return;

                        var user = new User
                        {
                            Email = email,
                            Password = passwordHasher.HashPassword(password),
                            FullName = fullName,
                            Status = UserStatus.Active,
                            Role = UserRole.SuperAdmin
                        };
                        context.Add(user);
                        context.SaveChanges();
                    });
            }
        );
        return services;
    }
}
