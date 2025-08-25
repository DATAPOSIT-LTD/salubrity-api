// File: Infrastructure/Security/Policies/SubcontractorOrAdminHandler.cs

using Microsoft.AspNetCore.Authorization;
using Salubrity.Application.Interfaces.Repositories.Users;
using System.Security.Claims;

public class SubcontractorOrAdminHandler : AuthorizationHandler<SubcontractorOrAdminRequirement>
{
    private readonly IUserRoleReadRepository _roles;

    public SubcontractorOrAdminHandler(IUserRoleReadRepository roles)
    {
        _roles = roles;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SubcontractorOrAdminRequirement requirement)
    {
        var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return;

        var hasRole = await _roles.HasAnyRoleAsync(userId, ["Admin", "Subcontractor"]);
        if (hasRole)
        {
            context.Succeed(requirement);
        }
    }
}
