using API.Interfaces.Response;
using Application.DTOs.Board.Board;
using Application.DTOs.Board.BoardMembership;
using Application.DTOs.Shared;
using Application.Errors;
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
            UserNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            BoardCodeAlreadyExistsError => ApiResponseHelper.Failure(errors, Conflict),
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                _ => Forbid()),
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
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                _ => Forbid()),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BoardDTO>>>> GetBoards()
    {
        var result = await boardService.GetBoards(UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        return ApiResponseHelper.Failure(
            result.Errors.Select(e => e.Message).ToList(),
            BadRequest
        );
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
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                _ => Forbid()),
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
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                _ => Forbid()),
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
                NotFound),
            BoardMembershipAlreadyExistsError => ApiResponseHelper.Failure(errors,
                Conflict),
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                _ => Forbid()),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize]
    [HttpGet("{boardId:int}/memberships")]
    public async Task<ActionResult<ApiResponse<IEnumerable<BoardMembershipDTO>>>>
        GetBoardMemberships(
            int boardId)
    {
        var result = await boardService.GetBoardMemberships(boardId, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            BoardNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                _ => Forbid()),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPut("memberships/{membershipId:int}")]
    public async Task<ActionResult<ApiResponse<BoardMembershipDTO>>>
        UpdateBoardMembership(
            int membershipId, UpdateBoardMembershipDTO dto)
    {
        var result =
            await boardService.UpdateBoardMembership(membershipId, dto, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            BoardMembershipNotFoundError or UserNotFoundError => ApiResponseHelper
                .Failure(errors, NotFound),
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                _ => Forbid()),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpDelete("memberships/{membershipId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveBoardMembership(
        int membershipId)
    {
        var result =
            await boardService.RemoveBoardMembership(membershipId, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success<object>(null, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            BoardMembershipNotFoundError => ApiResponseHelper.Failure(errors, NotFound),
            InsufficientUserPermissionsError => ApiResponseHelper.Failure(errors,
                _ => Forbid()),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }
}
