using FluentResults;

namespace Application.Errors;

public class BoardCodeAlreadyExistsError(string code)
    : Error($"A board with code '{code}' already exists.");
