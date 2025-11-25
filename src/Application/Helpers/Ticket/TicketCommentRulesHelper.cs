using Application.Errors;
using Domain.Enums;
using FluentResults;

namespace Application.Helpers.Ticket;

public static class TicketCommentRulesHelper
{
    public static Result CanDelete(MembershipRole role, bool isAuthor)
    {
        if (isAuthor) return Result.Ok();

        return role switch
        {
            MembershipRole.Owner => Result.Ok(),
            MembershipRole.Editor => Result.Ok(),
            _ => Result.Fail(
                new InsufficientUserMembershipPermissionsError($"{role} membership",
                    "Delete Comment"))
        };
    }

    public static Result CanEdit(bool isAuthor)
    {
        return isAuthor
            ? Result.Ok()
            : Result.Fail(
                new InsufficientUserMembershipPermissionsError(
                    "not the comment's author",
                    "Edit Comment"));
    }
}
