using ErpBackend.CrossCutting.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBackend.CrossCutting.Extensions;

/// <summary>
/// Registers permission-based authorization: the dynamic policy provider for
/// <c>[HasPermission("...")]</c> and the handler that checks the user's permission claims.
/// </summary>
public static class AuthorizationExtensions
{
    public static IServiceCollection AddErpAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        return services;
    }
}
