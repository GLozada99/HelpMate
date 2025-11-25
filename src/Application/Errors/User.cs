using FluentResults;

namespace Application.Errors;

public class UserNotFoundError(int id)
    : Error($"No user found with id: {id}.");

public class UserEmailAlreadyInUseError(string email)
    : Error($"Email '{email}' is already in use.");

public class UserInactiveInUseError(int id)
    : Error($"User with  id '{id}' is inactive.");

public class InsufficientUserPermissionsError(string role, string action)
    : Error(
        $"User with role '{role}' does not have the necessary permissions for this action: {action}.");

public class InsufficientUserMembershipPermissionsError(string context, string action)
    : Error(
        $"User with '{context}' does not have the necessary permissions for this action: {action}.");
