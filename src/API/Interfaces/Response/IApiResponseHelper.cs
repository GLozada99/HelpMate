using Application.DTOs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace API.Interfaces.Response;

public interface IApiResponseHelper
{
    ActionResult Failure(
        List<string>? errors,
        Func<object, ActionResult> responseBuilder,
        List<string>? messages = null);

    ActionResult<ApiResponse<T>> Success<T>(
        T? result,
        Func<ApiResponse<T>, ActionResult> responseBuilder,
        List<string>? messages = null
    );
}
