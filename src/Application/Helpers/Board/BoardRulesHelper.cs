using Application.Errors;
using Domain.Enums;
using FluentResults;

namespace Application.Helpers.Board;

public static class BoardRulesHelper
{
    public static Result CanCreateBoard(UserRole requesterRole)
    {
        return requesterRole switch
        {
            UserRole.SuperAdmin => Result.Ok(),
            UserRole.Admin => Result.Ok(),
            _ => Result.Fail(new InsufficientUserPermissionsError(
                requesterRole.ToString(),
                "Create Board"))
        };
    }

    public static Result CanUpdateBoard(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserMembershipPermissionsError(
                $"{requesterMembershipRole} membership",
                "Update Board"));
    }

    public static Result CanDeactivateBoard(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserMembershipPermissionsError(
                $"{requesterMembershipRole} membership",
                "Deactivate Board"));
    }

    public static Result CanCreateMembership(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserMembershipPermissionsError(
                $"{requesterMembershipRole} membership",
                "Create Board Membership"));
    }

    public static Result CanUpdateMembership(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserMembershipPermissionsError(
                $"{requesterMembershipRole} membership",
                "Update Board Membership"));
    }

    public static Result CanRemoveMembership(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserMembershipPermissionsError(
                $"{requesterMembershipRole} membership",
                "Remove Board Membership"));
    }

    public static Result CanRemoveMembershipConsideringLastOwner(
        MembershipRole targetMembershipRole,
        int ownersCount)
    {
        if (targetMembershipRole != MembershipRole.Owner)
            return Result.Ok();

        return ownersCount > 1
            ? Result.Ok()
            : Result.Fail(
                new InvalidBoardState("Cannot remove last owner of the board"));
    }

    public static Result BoardWithCodeExists(string boardCode,
        IQueryable<Domain.Entities.Board.Board> boards)
    {
        return boards.Any(b => b.Code == boardCode)
            ? Result.Fail(new BoardCodeAlreadyExistsError(boardCode))
            : Result.Ok();
    }

    public static Result CanHaveMembershipRole(UserRole role,
        MembershipRole membershipRole)
    {
        var allowedRoles = GetAllowedMembershipRoles(role);
        return allowedRoles.Contains(membershipRole)
            ? Result.Ok()
            : Result.Fail(new InsufficientUserPermissionsError(role.ToString(),
                $"Have a {membershipRole.ToString()} membership."));
    }

    private static List<MembershipRole> GetAllowedMembershipRoles(
        UserRole userRole)
    {
        return userRole switch
        {
            UserRole.Admin or UserRole.SuperAdmin =>
            [
                MembershipRole.Owner, MembershipRole.Agent, MembershipRole.Editor,
                MembershipRole.Viewer
            ],
            UserRole.Agent =>
                [MembershipRole.Agent, MembershipRole.Editor, MembershipRole.Viewer],
            UserRole.Customer => [MembershipRole.Viewer],
            _ => throw new ArgumentOutOfRangeException(nameof(userRole), userRole, null)
        };
    }
}
