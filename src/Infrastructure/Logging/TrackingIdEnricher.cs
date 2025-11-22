using Application.Interfaces.Tracking;
using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging;

public class TrackingIdEnricher(ITrackingIdProvider trackingIdProvider)
    : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("TrackingId",
                trackingIdProvider.GetTrackingId()));
    }
}
