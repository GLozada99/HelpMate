using FluentResults;

namespace Application.Errors;

public class BoardCodeAlreadyExistsError(string code)
    : Error($"A board with code '{code}' already exists.");

public class BoardNotFoundError(int id)
    : Error($"No board found with id: {id}.");

public class BoardMembershipAlreadyExistsError(int userId, int boardId)
    : Error($"User with id '{userId}' is already member of board with id: {boardId}.");

public class BoardMembershipNotFoundError(int id)
    : Error($"No board membership found with id: {id}.");
