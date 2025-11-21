using Application.DTOs.Response;

namespace Application.Helpers.Response;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T result,
        List<string>? messages = null, string? trackingId = null)
    {
        return new ApiResponse<T>
        {
            Result = result,
            Status = ResponseStatus.Success,
            Messages = messages ?? [],
            TrackingId = trackingId
        };
    }

    public static ApiResponse<object> Failure(
        List<string>? errors,
        List<string>? messages = null,
        string? trackingId = null)
    {
        return new ApiResponse<object>
        {
            Result = null,
            Status = ResponseStatus.Failure,
            Errors = errors ?? [],
            Messages = messages ?? [],
            TrackingId = trackingId
        };
    }
}
