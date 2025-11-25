using API.Interfaces.Response;
using Application.DTOs.Pagination;
using Application.DTOs.Shared;
using Application.DTOs.Ticket.TicketComment;
using Application.Errors;
using Application.Helpers.Pagination;
using Application.Interfaces.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace API.Controllers;

[ApiController]
[Route("api/boards/{boardId:int}/tickets/{ticketId:int}/comments")]
[OpenApiTag("Ticket Comments")]
[Authorize]
public class TicketCommentController(
    IApiResponseHelper apiResponseHelper,
    ITicketService ticketService
) : BaseController(apiResponseHelper)
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<TicketCommentDTO>>>>
        GetComments(
            int boardId,
            int ticketId,
            [FromQuery] PaginationQuery paginationDto)
    {
        var result = await ticketService.GetComments(boardId, ticketId, UserId);

        if (!result.IsSuccess)
            return ApiResponseHelper.Failure(
                result.Errors.Select(e => e.Message).ToList(),
                BadRequest
            );

        var paginated =
            PaginationHelper.GetPaginatedResult(result.Value, paginationDto);

        return ApiResponseHelper.Success(paginated, Ok);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TicketCommentDTO>>> AddComment(
        int boardId,
        int ticketId,
        CreateTicketCommentDTO dto)
    {
        var result =
            await ticketService.AddComment(boardId, ticketId, dto, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(
                result.Value,
                data => CreatedAtAction(nameof(GetComments),
                    new { boardId, ticketId }, data)
            );

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            TicketNotFoundError =>
                ApiResponseHelper.Failure(errors, NotFound),

            InsufficientUserMembershipPermissionsError =>
                ApiResponseHelper.Failure(errors,
                    r => new ObjectResult(r) { StatusCode = 403 }),

            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [HttpPatch("{commentId:int}")]
    public async Task<ActionResult<ApiResponse<TicketCommentDTO>>> UpdateComment(
        int boardId,
        int ticketId,
        int commentId,
        UpdateTicketCommentDTO dto)
    {
        var result =
            await ticketService.UpdateComment(boardId, ticketId, commentId, dto,
                UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            TicketCommentNotFoundError =>
                ApiResponseHelper.Failure(errors, NotFound),

            // Only comment author can edit
            InsufficientUserPermissionsError
                or InsufficientUserMembershipPermissionsError =>
                ApiResponseHelper.Failure(errors,
                    r => new ObjectResult(r) { StatusCode = 403 }),

            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [HttpDelete("{commentId:int}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteComment(
        int boardId,
        int ticketId,
        int commentId)
    {
        var result =
            await ticketService.DeleteComment(boardId, ticketId, commentId, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success<object>(null, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            TicketCommentNotFoundError =>
                ApiResponseHelper.Failure(errors, NotFound),

            // Owner/Editor can delete any; others can delete only their own
            InsufficientUserPermissionsError
                or InsufficientUserMembershipPermissionsError =>
                ApiResponseHelper.Failure(errors,
                    r => new ObjectResult(r) { StatusCode = 403 }),

            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }
}
