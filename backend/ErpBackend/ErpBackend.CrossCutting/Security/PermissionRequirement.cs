using Microsoft.AspNetCore.Authorization;

namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// Authorization requirement representing a single required permission.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission) => Permission = permission;
}
