using FluentResults;

namespace Application.Errors;

public class BoardCodeAlreadyExistsError(string code)
    : Error($"A board with code '{code}' already exists.");

public class BoardNotFoundError(int id)
    : Error($"No board found with id: {id}.");
