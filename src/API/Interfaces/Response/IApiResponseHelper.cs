using Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces.Response;

public interface IApiResponseHelper
{
    ActionResult<ApiResponse<T>> Success<T>(
        T? result,
        Func<ApiResponse<T>, ObjectResult> responseBuilder,
        List<string>? messages = null
    );

    ObjectResult Failure(
        List<string>? errors,
        Func<object, ObjectResult> responseBuilder,
        List<string>? messages = null);
}
