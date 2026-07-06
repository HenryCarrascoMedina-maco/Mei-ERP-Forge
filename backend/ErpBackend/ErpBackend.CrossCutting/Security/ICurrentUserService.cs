namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// Abstraction over the currently authenticated user, resolved from the HTTP request context.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }

    string? Email { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    IReadOnlyList<string> Roles { get; }

    IReadOnlyList<string> Permissions { get; }

    bool HasPermission(string permission);

    bool IsInRole(string role);
}
