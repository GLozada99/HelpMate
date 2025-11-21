using Application.Interfaces.Tracking;
using Infrastructure.Helpers.Tracking;
using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging;

public class TrackingIdEnricher(ITrackingIdHelper trackingIdHelper) : ILogEventEnricher
{
    public TrackingIdEnricher() : this(new TrackingIdHelper())
    {
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.ContainsKey("TrackingId"))
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TrackingId",
                    trackingIdHelper.GetTrackingId()));
    }
}
