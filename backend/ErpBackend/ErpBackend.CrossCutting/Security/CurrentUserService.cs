using ErpBackend.CrossCutting.Extensions;
using Microsoft.AspNetCore.Http;

namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// Default <see cref="ICurrentUserService"/> backed by <see cref="IHttpContextAccessor"/>.
/// Reads identity and authorization data from the request's claims principal.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    private System.Security.Claims.ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public string? UserId => User.GetUserId();

    public string? Email => User.GetEmail();

    public string? UserName => User.GetUserName();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles => User.GetRoles();

    public IReadOnlyList<string> Permissions => User.GetPermissions();

    public bool HasPermission(string permission) => User.HasPermission(permission);

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
