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
            : Result.Fail(new InsufficientUserPermissionsError(
                requesterMembershipRole.ToString(),
                "Update Board"));
    }

    public static Result CanDeactivateBoard(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserPermissionsError(
                requesterMembershipRole.ToString(),
                "Deactivate Board"));
    }

    public static Result CanCreateMembership(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserPermissionsError(
                requesterMembershipRole.ToString(),
                "Create Board Membership"));
    }

    public static Result CanUpdateMembership(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserPermissionsError(
                requesterMembershipRole.ToString(),
                "Update Board Membership"));
    }

    public static Result CanRemoveMembership(MembershipRole requesterMembershipRole)
    {
        return requesterMembershipRole == MembershipRole.Owner
            ? Result.Ok()
            : Result.Fail(new InsufficientUserPermissionsError(
                requesterMembershipRole.ToString(),
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
            : Result.Fail("Cannot remove last owner of the board");
    }
}
