using System.Security.Claims;
using Domain.Enums;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Startup;

public static class AuthConfigs
{
    public static IServiceCollection ConfigureCookies(this IServiceCollection services)
    {
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "HelpMateAuth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                options.Events.OnValidatePrincipal = async context =>
                {
                    var userIdStr = context.Principal
                        ?.FindFirst(ClaimTypes.NameIdentifier)
                        ?.Value;
                    if (!int.TryParse(userIdStr, out var userId))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(
                            CookieAuthenticationDefaults
                                .AuthenticationScheme);
                        return;
                    }

                    var db = context.HttpContext.RequestServices
                        .GetRequiredService<HelpMateDbContext>();
                    var user = await db.Users.FindAsync(userId);

                    if (user == null)
                        return;

                    // Log out the user if it is not active
                    if (user is not { Status: UserStatus.Active })
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync(
                            CookieAuthenticationDefaults
                                .AuthenticationScheme);
                    }
                };
            });


        return services;
    }

    public static IServiceCollection ConfigureCoors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("allowAll", policy =>
            {
                policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        return services;
    }

    public static IServiceCollection ConfigurePolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("IsAdmin",
                policy => policy.RequireClaim(ClaimTypes.Role, "Admin"))
            .AddPolicy("IsAgent",
                policy => policy.RequireClaim(ClaimTypes.Role, "Agent"))
            .AddPolicy("IsCustomer",
                policy => policy.RequireClaim(ClaimTypes.Role, "Customer"))
            .AddPolicy("IsStaff",
                policy =>
                {
                    policy.RequireAssertion(context => context.User.HasClaim(c => c is
                    {
                        Type: ClaimTypes.Role, Value: "Admin" or "Agent"
                    }));
                });

        return services;
    }
}
