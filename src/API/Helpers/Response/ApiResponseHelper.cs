using API.Interfaces.Response;
using Application.DTOs.Response;
using Application.Helpers.Response;
using Infrastructure.Helpers.Tracking;
using Microsoft.AspNetCore.Mvc;

namespace API.Helpers.Response;

public class ApiResponseHelper(TrackingIdHelper trackingIdHelper) : IApiResponseHelper
{
    public ActionResult<ApiResponse<T>> Success<T>(
        T? result,
        Func<ApiResponse<T>, ObjectResult> responseBuilder,
        List<string>? messages = null
    )
    {
        var wrapped =
            ApiResponseFactory.Success(result!, messages,
                trackingIdHelper.GetTrackingId());
        return responseBuilder(wrapped);
    }

    public ObjectResult Failure(
        List<string>? errors,
        Func<object, ObjectResult> responseBuilder,
        List<string>? messages = null)
    {
        var wrapped =
            ApiResponseFactory.Failure(errors, messages,
                trackingIdHelper.GetTrackingId());
        return responseBuilder(wrapped);
    }
}
