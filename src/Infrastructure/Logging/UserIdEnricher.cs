using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging;

public class UserIdEnricher(IHttpContextAccessor contextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = contextAccessor.HttpContext;
        if (httpContext == null)
            return;

        var userIdStr = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return;

        if (int.TryParse(userIdStr, out var userId))
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("UserId", userId));
    }
}
