using Application.Errors;
using Domain.Enums;
using FluentResults;

namespace Application.Helpers.Ticket;

public static class TicketRulesHelper
{
    public static Result CanCreateTicket(MembershipRole role)
    {
        return role == MembershipRole.Viewer
            ? Result.Fail(
                new InsufficientUserMembershipPermissionsError($"{role} membership",
                    "Create Ticket"))
            : Result.Ok();
    }

    public static Result CanEditTicket(MembershipRole role)
    {
        return role == MembershipRole.Viewer
            ? Result.Fail(
                new InsufficientUserMembershipPermissionsError($"{role} membership",
                    "Edit Ticket"))
            : Result.Ok();
    }

    public static Result CanBeReporter(MembershipRole role)
    {
        return role == MembershipRole.Viewer
            ? Result.Fail(
                new InsufficientUserMembershipPermissionsError($"{role} membership",
                    "Be Reporter"))
            : Result.Ok();
    }

    public static Result CanBeAssigned(MembershipRole role)
    {
        return role switch
        {
            MembershipRole.Agent => Result.Ok(),
            MembershipRole.Owner => Result.Ok(),
            _ => Result.Fail(
                new InsufficientUserMembershipPermissionsError($"{role} membership",
                    "Be Assigned"))
        };
    }
}
