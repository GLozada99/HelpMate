using System.Security.Claims;
using API.Interfaces.Response;
using Application.DTOs.Auth;
using Application.DTOs.Shared;
using Application.Errors;
using Application.Interfaces.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
[OpenApiTag("Authentication")]
public class AuthController(
    IApiResponseHelper apiResponseHelper,
    IAuthService authService)
    : BaseController(apiResponseHelper)
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoggedInUserDTO>>> Login(LoginDTO dto)
    {
        var result = await authService.ValidateUserLogin(dto);

        if (result.IsFailed)
            // Regardless of the error, the response is 401 "Invalid credentials" to
            // avoid giving information to an attacked regarding the credentials provided.
            return ApiResponseHelper.Failure(["Invalid credentials"], Unauthorized);

        var userDto = result.Value;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new(ClaimTypes.Email, userDto.Email),
            new(ClaimTypes.Name, userDto.FullName),
            new(ClaimTypes.Role, userDto.Role.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );
        return ApiResponseHelper.Success(result.Value, Ok);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return ApiResponseHelper.Success<object>(null, Ok,
            ["User logged out successfully"]);
    }

    [HttpGet("user-info")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<LoggedInUserDTO>>> GetUserInfo()
    {
        var userId = GetUserId();
        var result = await authService.GetUserInfo(userId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        // Should always be a success, as it is using the id of the authenticated user
        // but handling errors just in case
        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }
}
