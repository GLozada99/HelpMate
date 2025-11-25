using API.Interfaces.Response;
using Application.DTOs.Pagination;
using Application.DTOs.Shared;
using Application.DTOs.Ticket.Ticket;
using Application.Errors;
using Application.Helpers.Pagination;
using Application.Interfaces.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace API.Controllers;

[ApiController]
[Route("api/boards/{boardId:int}/tickets")]
[OpenApiTag("Tickets")]
[Authorize]
public class TicketController(
    IApiResponseHelper apiResponseHelper,
    ITicketService ticketService
) : BaseController(apiResponseHelper)
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TicketDTO>>> CreateTicket(
        int boardId,
        CreateTicketDTO dto)
    {
        var result = await ticketService.CreateTicket(boardId, dto, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value,
                data => CreatedAtAction(nameof(GetTicket),
                    new { boardId, ticketId = data.Result!.Id }, data));

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            UserNotFoundError or BoardNotFoundError =>
                ApiResponseHelper.Failure(errors, BadRequest),
            InsufficientUserMembershipError =>
                ApiResponseHelper.Failure(errors,
                    response => new ObjectResult(response) { StatusCode = 403 }),
            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [HttpGet("{ticketId:int}")]
    public async Task<ActionResult<ApiResponse<TicketDTO>>> GetTicket(
        int boardId,
        int ticketId)
    {
        var result = await ticketService.GetTicket(boardId, ticketId, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors.Select(e => e.Message).ToList();

        return error switch
        {
            TicketNotFoundError =>
                ApiResponseHelper.Failure(errors, NotFound),

            InsufficientUserMembershipError =>
                ApiResponseHelper.Failure(errors,
                    response => new ObjectResult(response) { StatusCode = 403 }),

            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<TicketListDTO>>>>
        GetTickets(
            int boardId,
            [FromQuery] PaginationQuery paginationDto)
    {
        var result = await ticketService.GetTickets(boardId, UserId);

        if (!result.IsSuccess)
            return ApiResponseHelper.Failure(
                result.Errors.Select(e => e.Message).ToList(),
                BadRequest
            );

        var paginated =
            PaginationHelper.GetPaginatedResult(result.Value, paginationDto);

        return ApiResponseHelper.Success(paginated, Ok);
    }

    [HttpPatch("{ticketId:int}")]
    public async Task<ActionResult<ApiResponse<TicketDTO>>> UpdateTicket(
        int boardId,
        int ticketId,
        UpdateTicketDTO dto)
    {
        var result =
            await ticketService.UpdateTicket(boardId, ticketId, dto, UserId);

        if (result.IsSuccess)
            return ApiResponseHelper.Success(result.Value, Ok);

        var error = result.Errors[0];
        var errors = result.Errors
            .Select(e => e.Message)
            .ToList();

        return error switch
        {
            TicketNotFoundError =>
                ApiResponseHelper.Failure(errors, NotFound),

            UserNotFoundError or BoardNotFoundError =>
                ApiResponseHelper.Failure(errors, BadRequest),

            InsufficientUserMembershipError or InsufficientUserPermissionsError =>
                ApiResponseHelper.Failure(
                    errors,
                    response => new ObjectResult(response) { StatusCode = 403 }),

            _ => ApiResponseHelper.Failure(errors, BadRequest)
        };
    }
}
