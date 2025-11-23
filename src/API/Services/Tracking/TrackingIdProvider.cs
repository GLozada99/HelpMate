using Application.Interfaces.Tracking;

namespace API.Services.Tracking;

public class TrackingIdProvider : ITrackingIdProvider
{
    private readonly AsyncLocal<string?> _current = new();

    public string GetTrackingId()
    {
        _current.Value ??= Guid.NewGuid().ToString();

        return _current.Value;
    }
}
