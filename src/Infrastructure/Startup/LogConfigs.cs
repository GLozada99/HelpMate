using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Templates;

namespace LubricambioBackend.Infrastructure.Startup;

public static class LogConfigs
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder host)
    {
        return host;
    }

    public static ILoggingBuilder ConfigureLogging(this ILoggingBuilder logging)
    {
        return logging;
    }
}
