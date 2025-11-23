using FluentResults;

namespace Application.Errors;

public class NoUserWithEmailError(string email)
    : Error($"No user with email '{email}' was found.");

public class InvalidCredentialsError()
    : Error("Password is invalid.");

public class NotActiveUserError()
    : Error("User is not active.");
