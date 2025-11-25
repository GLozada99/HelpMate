using FluentResults;

namespace Application.Errors;

public class TicketNotFoundError(int id)
    : Error($"No ticket found with id: {id}.");

public class TicketCommentNotFoundError(int id)
    : Error($"No ticket comment found with id: {id}.");
