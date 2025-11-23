using API.Interfaces.Response;
using Application.DTOs.Shared;
using Application.Interfaces.Tracking;
using Microsoft.AspNetCore.Mvc;

namespace API.Helpers.Response;

public class ApiResponseHelper(ITrackingIdProvider trackingIdProvider)
    : IApiResponseHelper
{
    public ObjectResult Failure(
        List<string>? errors,
        Func<object, ObjectResult> responseBuilder,
        List<string>? messages = null)
    {
        var wrapped =
            ApiResponseFactory.Failure(errors, messages,
                trackingIdProvider.GetTrackingId());
        return responseBuilder(wrapped);
    }

    public ActionResult<ApiResponse<T>> Success<T>(
        T? result,
        Func<ApiResponse<T>, ObjectResult> responseBuilder,
        List<string>? messages = null
    )
    {
        var wrapped =
            ApiResponseFactory.Success(result, messages,
                trackingIdProvider.GetTrackingId());
        return responseBuilder(wrapped);
    }
}
