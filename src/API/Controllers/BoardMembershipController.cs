using API.Interfaces.Response;
using Application.DTOs.Board.BoardMembership;
using Application.DTOs.Pagination;
using Application.DTOs.Shared;
using Application.Errors;
using Application.Helpers.Pagination;
using Application.Interfaces.Board;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace API.Controllers;

[ApiController]
[Route("api/boards/{boardId:int}/memberships")]
[OpenApiTag("BoardMemberships")]
[Authorize]
public class BoardMembershipController(
    IApiResponseHelper apiResponseHelper,
    IBoardService boardService)
    : BaseController(apiResponseHelper)
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BoardMembershipDTO>>>
        CreateBoardMembership(
            int boardId, CreateBoardMembershipDTO dto)
    {
        var result =
            await boardService.CreateBoardMembership(boardId, dto, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value,
                data => CreatedAtAction(nameof(GetBoardMemberships),
                    new { boardId }, data));

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError or BoardNotFoundError => ApiResponseHelper.Failure(errors,
                BadRequest),
            BoardMembershipAlreadyExistsError => ApiResponseHelper.Failure(errors,
                Conflict),
            InsufficientUserMembershipError => ApiResponseHelper.Failure(errors,
                response => new ObjectResult(response) { StatusCode = 403 }),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<BoardMembershipDTO>>>>
        GetBoardMemberships(
            int boardId, [FromQuery] PaginationQuery paginationDto
        )
    {
        var result = await boardService.GetBoardMemberships(boardId, UserId);

        if (!result.IsSuccess)
            return ApiResponseHelper.Failure(
                result.Errors.Select(e => e.Message).ToList(),
                BadRequest
            );

        var paginated =
            PaginationHelper.GetPaginatedResult(result.Value, paginationDto);
        return ApiResponseHelper.Success(paginated, Ok);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPatch("{userId:int}")]
    public async Task<ActionResult<ApiResponse<BoardMembershipDTO>>>
        UpdateBoardMembership(
            int boardId, int userId, UpdateBoardMembershipDTO dto)
    {
        var result =
            await boardService.UpdateBoardMembership(boardId, userId, dto, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError => ApiResponseHelper.Failure(errors,
                BadRequest),
            BoardMembershipNotFoundError => ApiResponseHelper
                .Failure(errors, NotFound),
            InsufficientUserMembershipError or InsufficientUserPermissionsError =>
                ApiResponseHelper.Failure(errors,
                    response => new ObjectResult(response) { StatusCode = 403 }),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{userId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBoardMembership(
        int boardId, int userId)
    {
        var result =
            await boardService.DeleteBoardMembership(boardId, userId, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success<object>(null, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError => ApiResponseHelper.Failure(errors,
                BadRequest),
            BoardMembershipNotFoundError => ApiResponseHelper
                .Failure(errors, NotFound),
            InsufficientUserMembershipError or InsufficientUserPermissionsError =>
                ApiResponseHelper.Failure(errors,
                    response => new ObjectResult(response) { StatusCode = 403 }),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }
}
