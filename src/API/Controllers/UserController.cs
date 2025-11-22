using API.Interfaces.Response;
using Application.DTOs.Pagination;
using Application.DTOs.Shared;
using Application.DTOs.User;
using Application.Errors;
using Application.Helpers.Pagination;
using Application.Interfaces.User;
using Domain.Enums;
using Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
[OpenApiTag("Users")]
public class UserController(
    IApiResponseHelper apiResponseHelper,
    IUserService userService)
    : BaseController(apiResponseHelper)
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDTO>>> CreateUser(CreateUserDTO dto)
    {
        var result = await userService.CreateUser(dto);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value,
                data => CreatedAtAction(nameof(GetUser), new { id = data.Result!.Id },
                    data));

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserEmailAlreadyInUseError => ApiResponseHelper.Failure(errors, Conflict),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDTO>>> GetUser(int id)
    {
        var result = await userService.GetUser(id);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateUser(int id,
        UpdateUserDTO dto)
    {
        var result = await userService.UpdateUser(id, dto);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateUser(int id)
    {
        var result = await userService.DeactivateUser(id);

        if (result.IsSuccess)
            return ApiResponseHelper.Success<object>(null, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<UserDTO>>>> GetUsers(
        [FromQuery] GetUserQueryDTO dto)
    {
        {
            var result = await userService.GetUsers(dto);

            if (!result.IsSuccess)
                return ApiResponseHelper.Failure(
                    result.Errors.Select(e => e.Message).ToList(),
                    BadRequest);

            var paginated = PaginationHelper.GetPaginatedResult(result.Value, dto);
            return ApiResponseHelper.Success(paginated, Ok);
        }
    }

    [OpenApiOperation("Create a SuperAdminUser.",
        """
        Endpoint to bypass having an initial user.<br/>
        Does not required authentication.<br/>
        The role provided will be ignored.<br/>
        For demonstration purposes only.<br/>
        MUST be removed in Production.
        """)]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("/super-admin")]
    public async Task<ActionResult<ApiResponse<UserDTO>>> CreateSuperAdminUser(
        CreateUserDTO dto, HelpMateDbContext context)
    {
        var result = await userService.CreateUser(dto);


        if (result.IsSuccess)
        {
            (await context.Users.FindAsync(result.Value.Id))!.Role =
                UserRole.SuperAdmin;
            await context.SaveChangesAsync();

            return ApiResponseHelper.Success(result.Value,
                data => CreatedAtAction(nameof(GetUser), new { id = data.Result!.Id },
                    data));
        }

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserEmailAlreadyInUseError => ApiResponseHelper.Failure(errors, Conflict),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }
}
