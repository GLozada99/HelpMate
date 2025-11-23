using System.Security.Claims;
using API.Interfaces.Response;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BaseController(IApiResponseHelper apiResponseHelper) : ControllerBase
{
    protected IApiResponseHelper ApiResponseHelper => apiResponseHelper;

    protected int GetUserId()
    {
        if (User is not { Identity.IsAuthenticated: true })
            throw new Exception("Calling GetUserId from an unauthenticated context");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            throw new Exception(
                "User has authenticated identity but no NameIdentifier in claims");

        return int.Parse(userId);
    }
}
