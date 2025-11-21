using Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Templates;

namespace Infrastructure.Startup;

public static class LogConfigs
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder host,
        IConfigurationManager configuration)
    {
        host.UseSerilog((context, services, logger) =>
        {
            var fileFormatter = new ExpressionTemplate(
                "{ {Timestamp: @t, Level: @l, Message: @mt, Exception: @x, Properties: " +
                "{..@p, ConnectionId: undefined(), RequestId: undefined(), ColumnNumber: " +
                "undefined(), EventId: undefined(), ActionName: undefined(), SourceContext: " +
                "undefined(), ActionId: undefined()}} }");
            const string consoleTemplate =
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}]" +
                " [TrackingId:{TrackingId}] {Message:lj}{NewLine}{Exception}";
            var logPath = configuration.GetSection("Logs")["Path"] ?? "/tmp/api.json";

            logger
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.With<TrackingIdEnricher>()
                .Enrich.With<UserIdEnricher>()
                .Enrich.WithCallerInfo(true,
                    ["Domain", "Application", "Infrastructure", "API"])
                .WriteTo.Console(outputTemplate: consoleTemplate)
                .WriteTo.File(fileFormatter, logPath, buffered: false);
        });

        return host;
    }

    public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        return logging;
    }
}
