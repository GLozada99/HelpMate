using Application.Interfaces.Tracking;

namespace Infrastructure.Helpers.Tracking;

public class TrackingIdHelper : ITrackingIdHelper
{
    private readonly AsyncLocal<string?> _current = new();
    private readonly ITrackingIdHelper _trackingIdHelper;

    public TrackingIdHelper() : this(new TrackingIdHelper())
    {
    }

    public TrackingIdHelper(ITrackingIdHelper trackingIdHelper)
    {
        _trackingIdHelper = trackingIdHelper;
    }

    private string Current
    {
        get
        {
            if (string.IsNullOrEmpty(_current.Value))
                _current.Value = Guid.NewGuid().ToString();

            return _current.Value;
        }
    }


    public string GetTrackingId()
    {
        return Current;
    }
}
