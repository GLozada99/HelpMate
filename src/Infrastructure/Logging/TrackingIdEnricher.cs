using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging;

public class TrackingIdEnricher : ILogEventEnricher
{
    private static readonly AsyncLocal<string?> _current = new();

    public static string Current
    {
        get
        {
            if (string.IsNullOrEmpty(_current.Value))
                _current.Value = Guid.NewGuid().ToString();

            return _current.Value;
        }
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.ContainsKey("TrackingId"))
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("TrackingId", Current));
    }

    public static void Reset()
    {
        _current.Value = null;
    }
}
