using Application.DTOs.User;
using Domain.Enums;
using FluentResults;

namespace Application.Errors;

public class UserNotFoundError(int id)
    : Error($"No user found with id: {id}.");

public class UserEmailAlreadyInUseError(string email)
    : Error($"Email '{email}' is already in use.");

public class UserInvalidParamUpdateError(
    string fieldName,
    string newValue)
    : Error($"User field '{fieldName}' cannot be set to '{newValue}'.");

public class UserInvalidStatusUpdateError(
    UserStatus status,
    UpdateUserStatus updateUserStatus)
    : Error($"User status '{status}' cannot be set to '{updateUserStatus}'.");

public class UserInvalidRoleUpdateError(
    UserRole role,
    UpdateUserRole updateUserRole)
    : Error($"User role '{role}' cannot be set to '{updateUserRole}'.");
