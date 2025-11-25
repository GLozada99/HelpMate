using API.Interfaces.Response;
using Application.DTOs.Board.Board;
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
[Route("api/boards")]
[OpenApiTag("Boards")]
[Authorize]
public class BoardController(
    IApiResponseHelper apiResponseHelper,
    IBoardService boardService)
    : BaseController(apiResponseHelper)
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BoardDTO>>> CreateBoard(
        CreateBoardDTO dto)
    {
        var result = await boardService.CreateBoard(dto, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value,
                data => CreatedAtAction(nameof(GetBoard),
                    new { id = data.Result!.Id }, data));

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError => ApiResponseHelper.Failure(errors, BadRequest),
            BoardCodeAlreadyExistsError => ApiResponseHelper.Failure(errors, Conflict),
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                response => new ObjectResult(response) { StatusCode = 403 }),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BoardDTO>>> GetBoard(int id)
    {
        var result = await boardService.GetBoard(id, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            BoardNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            InsufficientUserMembershipError => ApiResponseHelper.Failure(errors,
                response => new ObjectResult(response) { StatusCode = 403 }),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<BoardDTO>>>> GetBoards(
        [FromQuery] PaginationQuery paginationDto)
    {
        var result = await boardService.GetBoards(UserId);

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
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<BoardDTO>>> UpdateBoard(
        int id, UpdateBoardDTO dto)
    {
        var result = await boardService.UpdateBoard(id, dto, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            BoardNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            InsufficientUserMembershipError => ApiResponseHelper.Failure(errors,
                response => new ObjectResult(response) { StatusCode = 403 }),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeactivateBoard(int id)
    {
        var result = await boardService.DeactivateBoard(id, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success<object>(null, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            BoardNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            InsufficientUserMembershipError => ApiResponseHelper.Failure(errors,
                response => new ObjectResult(response) { StatusCode = 403 }),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost("{boardId:int}/memberships")]
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
    [HttpGet("{boardId:int}/memberships")]
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
    [HttpPut("{boardId:int}/memberships/{userId:int}")]
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
    [HttpDelete("{boardId:int}/memberships/{userId:int}")]
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
