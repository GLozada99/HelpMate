using FluentResults;

namespace Application.Errors;

public class UserNotFoundError(int id)
    : Error($"No user found with id: {id}.");

public class UserEmailAlreadyInUseError(string email)
    : Error($"Email '{email}' is already in use.");

public class InsufficientUserPermissionsError(string role, string action)
    : Error(
        $"User with role '{role}' does not have the necessary permissions for this action: {action}.");

public class InsufficientUserMembershipError(string membershipRole, string action)
    : Error(
        $"User with membership '{membershipRole}' does not have the necessary permissions for this action: {action}.");
