using Application.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces.Response;

public interface IApiResponseHelper
{
    ObjectResult Failure(
        List<string>? errors,
        Func<object, ObjectResult> responseBuilder,
        List<string>? messages = null);

    ActionResult<ApiResponse<T>> Success<T>(
        T? result,
        Func<ApiResponse<T>, ObjectResult> responseBuilder,
        List<string>? messages = null
    );
}
