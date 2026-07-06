using ErpBackend.CrossCutting.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// Validates that the authenticated user carries the permission claim required by a
/// <see cref="PermissionRequirement"/>.
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
