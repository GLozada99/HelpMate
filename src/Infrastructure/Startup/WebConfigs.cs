using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Startup;

public static class WebConfigs
{
    public static IServiceCollection ConfigureWeb(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy =
                JsonNamingPolicy.CamelCase;
        });
        return services;
    }

    public static WebApplication ConfigureWebApp(this WebApplication app)
    {
        app.UseCors("allowAll");
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseStatusCodePages();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.MapControllers();
        app.UseStatusCodePages();

        return app;
    }

    public static WebApplication ConfigureSwagger(this WebApplication app)
    {
        app.UseOpenApi();
        app.UseSwaggerUi();

        app.MapGet("/", () => Results.Redirect("/swagger"));

        return app;
    }

    public static IServiceCollection ConfigureApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument();
        return services;
    }
}
