using FluentResults;

namespace Application.Errors;

public class TicketNotFoundError(int id)
    : Error($"No ticket found with id: {id}.");
