using System.Security.Claims;
using Domain.Entities.User;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.Helpers;

public class ClaimsHelper
{
    public static ClaimsPrincipal GetClaim(Dictionary<string, string> claimsMap)
    {
        var result = claimsMap.Select(claim => new Claim(claim.Key, claim.Value))
            .ToList();
        var fakeIdentity = new ClaimsIdentity(result, "TestAuthType");
        return new ClaimsPrincipal(fakeIdentity);
    }

    public static User InsertUserInClaims(ControllerBase controller,
        bool isAdmin = false)
    {
        var role = isAdmin ? UserRole.SuperAdmin : UserRole.Agent;
        var user = new User
        {
            Email = "TestName",
            Password = "test@test.com",
            FullName = "Some Password",
            Role = UserRole.SuperAdmin
        };
        return InsertUserInClaims(controller, user);
    }

    public static User InsertUserInClaims(ControllerBase controller, User user)
    {
        controller.ControllerContext.HttpContext = new DefaultHttpContext
        {
            User = GetClaim(new Dictionary<string, string>
                { { ClaimTypes.NameIdentifier, user.Id.ToString() } })
        };
        return user;
    }
}
