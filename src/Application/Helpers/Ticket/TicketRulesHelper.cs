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
                new InsufficientUserMembershipError(role.ToString(), "Create Ticket"))
            : Result.Ok();
    }

    public static Result CanEditTicket(MembershipRole role)
    {
        return role == MembershipRole.Viewer
            ? Result.Fail(
                new InsufficientUserMembershipError(role.ToString(), "Edit Ticket"))
            : Result.Ok();
    }

    public static Result CanBeReporter(MembershipRole role)
    {
        return role == MembershipRole.Viewer
            ? Result.Fail(
                new InsufficientUserMembershipError(role.ToString(), "Be Reporter"))
            : Result.Ok();
    }

    public static Result CanBeAssigned(MembershipRole role)
    {
        return role switch
        {
            MembershipRole.Agent => Result.Ok(),
            MembershipRole.Owner => Result.Ok(),
            _ => Result.Fail(
                new InsufficientUserMembershipError(role.ToString(), "Be Assigned"))
        };
    }

    public static Result CanAssignUsers(MembershipRole role)
    {
        return role == MembershipRole.Viewer
            ? Result.Fail(
                new InsufficientUserMembershipError(role.ToString(), "Assign Users"))
            : Result.Ok();
    }
}
