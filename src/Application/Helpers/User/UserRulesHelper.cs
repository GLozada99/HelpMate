using Application.Errors;
using Domain.Enums;
using FluentResults;

namespace Application.Helpers.User;

public static class UserRulesHelper
{
    public static Result CanCreateUser(UserRole requesterRole, UserRole newUserRole)
    {
        return (requesterRole, newUserRole) switch
        {
            (UserRole.SuperAdmin, _) => Result.Ok(),

            (UserRole.Admin, var role) when role != UserRole.SuperAdmin => Result.Ok(),

            _ => Result.Fail(
                new InsufficientUserPermissionsError(
                    requesterRole.ToString(),
                    $"Create '{newUserRole}' User"))
        };
    }

    public static Result CanUpdateUser(UserRole requesterRole, UserRole targetUserRole)
    {
        return (requesterRole, targetUserRole) switch
        {
            (UserRole.SuperAdmin, _) => Result.Ok(),

            (UserRole.Admin, var role) when role != UserRole.SuperAdmin => Result.Ok(),

            _ => Result.Fail(
                new InsufficientUserPermissionsError(
                    requesterRole.ToString(),
                    $"Update '{targetUserRole}' User"))
        };
    }

    public static Result CanUpdateUserRole(
        UserRole requesterRole,
        UserRole currentUserRole,
        UserRole newUserRole)
    {
        return (requesterRole, currentUserRole, newUserRole) switch
        {
            (UserRole.SuperAdmin, _, _) => Result.Ok(),

            (UserRole.Admin, UserRole.SuperAdmin, _) =>
                Result.Fail(
                    new InsufficientUserPermissionsError("Admin", "Modify SuperAdmin")),

            (UserRole.Admin, _, UserRole.SuperAdmin) =>
                Result.Fail(
                    new InsufficientUserPermissionsError("Admin",
                        "Assign SuperAdmin role")),

            (UserRole.Admin, _, _) => Result.Ok(),

            _ => Result.Fail(
                new InsufficientUserPermissionsError(
                    requesterRole.ToString(),
                    $"Update role for '{currentUserRole}' User"))
        };
    }

    public static Result CanDeactivateUser(UserRole requesterRole,
        UserRole targetUserRole)
    {
        return (requesterRole, targetUserRole) switch
        {
            (UserRole.SuperAdmin, _) => Result.Ok(),

            (UserRole.Admin, var role) when role != UserRole.SuperAdmin => Result.Ok(),

            _ => Result.Fail(
                new InsufficientUserPermissionsError(
                    requesterRole.ToString(),
                    $"Deactivate '{targetUserRole}' User"))
        };
    }

    public static UserRole MapRole(CreateUserRole role)
    {
        return role switch
        {
            CreateUserRole.Customer => UserRole.Customer,
            CreateUserRole.Agent => UserRole.Agent,
            CreateUserRole.Admin => UserRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };
    }

    public static UserRole MapRole(UpdateUserRole role)
    {
        return role switch
        {
            UpdateUserRole.Customer => UserRole.Customer,
            UpdateUserRole.Agent => UserRole.Agent,
            UpdateUserRole.Admin => UserRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };
    }
}
