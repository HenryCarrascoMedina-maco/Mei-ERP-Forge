using ErpBackend.Identity.Configuration;
using ErpBackend.Identity.Controllers;
using ErpBackend.Identity.Seeding;
using ErpBackend.Identity.Services;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registration entry point for the ERP Platform identity layer.</summary>
public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// Registers identity options, the auth service, the default-data seeder and the identity
    /// controllers (as an MVC application part). Assumes <c>AddErpPersistence</c> (the DbContext) and
    /// <c>AddErpJwtAuthentication</c>/<c>AddErpAuthorization</c> are configured by the host.
    /// </summary>
    /// <param name="services">The DI service collection.</param>
    /// <param name="configuration">App configuration (reads the <c>"Identity"</c> section).</param>
    public static IServiceCollection AddErpIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ErpIdentityOptions>(configuration.GetSection(ErpIdentityOptions.SectionName));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddErpSeeder<DefaultIdentitySeeder>();

        // Make the identity controllers (in this assembly) discoverable by MVC in the host app.
        services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);

        return services;
    }
}
