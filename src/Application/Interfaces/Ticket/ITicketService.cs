using Application.DTOs.Tickets;
using FluentResults;

namespace Application.Interfaces.Ticket;

public interface ITicketService
{
    Task<Result<TicketDTO>> CreateTicket(int boardId, CreateTicketDTO dto,
        int requesterId);

    Task<Result<TicketDTO>> GetTicket(int boardId, int ticketId, int requesterId);

    Task<Result<IQueryable<TicketListDTO>>> GetTickets(int boardId, int requesterId);

    Task<Result<TicketDTO>> UpdateTicket(int boardId, int ticketId, UpdateTicketDTO dto,
        int requesterId);
}
