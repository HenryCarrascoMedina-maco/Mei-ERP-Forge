using ErpBackend.CrossCutting.Helpers;
using ErpBackend.CrossCutting.Security;
using ErpBackend.Identity.Configuration;
using ErpBackend.Identity.Entities;
using ErpBackend.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBackend.Identity.Seeding;

/// <summary>
/// Idempotent seeder for the base security data: the Administrator and ReadOnly roles, and the
/// initial admin user (credentials from <see cref="ErpIdentityOptions"/> / configuration — never
/// hardcoded). Safe to run on every startup; only inserts what is missing.
/// </summary>
public sealed class DefaultIdentitySeeder(
    DbContext context,
    IOptions<ErpIdentityOptions> options,
    ILogger<DefaultIdentitySeeder> logger) : IErpDataSeeder
{
    private readonly ErpIdentityOptions _options = options.Value;

    /// <summary>Runs before business-data seeders.</summary>
    public int Order => 0;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var adminPermissions = PermissionCatalog.Platform
            .Concat(_options.AdditionalAdminPermissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // First run: create the base roles with the full admin permission set.
        if (!await context.Set<Role>().AnyAsync(cancellationToken))
        {
            context.Set<Role>().AddRange(
                new Role { Name = RoleConstants.Administrator, Description = "Full access.", Permissions = adminPermissions },
                new Role { Name = RoleConstants.ReadOnly, Description = "Read-only access.", Permissions = PermissionCatalog.ReadOnly.ToList() });

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded base roles: {Roles}.", $"{RoleConstants.Administrator}, {RoleConstants.ReadOnly}");
            return;
        }

        // Subsequent runs: reconcile the Administrator role so newly-declared permissions (e.g. a
        // freshly generated module added to Identity:AdditionalAdminPermissions) are granted on the
        // next startup. Append-only and idempotent — never removes permissions, never touches custom
        // roles. Without this, generating a module would 403 until the admin role was edited by hand.
        var adminRole = await context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == RoleConstants.Administrator, cancellationToken);
        if (adminRole is null)
        {
            return;
        }

        var missing = adminPermissions
            .Where(p => !adminRole.Permissions.Contains(p, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (missing.Count == 0)
        {
            return;
        }

        // Reassign (not in-place mutate) so the change is tracked regardless of the collection mapping.
        adminRole.Permissions = adminRole.Permissions.Concat(missing).ToList();
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Granted {Count} new permission(s) to the {Role} role: {Permissions}.",
            missing.Count, RoleConstants.Administrator, string.Join(", ", missing));
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<User>().IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var admin = _options.DefaultAdmin;
        if (string.IsNullOrWhiteSpace(admin.Password))
        {
            logger.LogWarning(
                "No initial admin seeded: set Identity:DefaultAdmin:Password (config/env) to enable admin seeding.");
            return;
        }

        var adminRole = await context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == RoleConstants.Administrator, cancellationToken);
        if (adminRole is null)
        {
            logger.LogWarning("Administrator role not found; skipping admin user seeding.");
            return;
        }

        var user = new User
        {
            UserName = admin.UserName,
            Email = admin.Email,
            DisplayName = admin.UserName,
            PasswordHash = PasswordHelper.Hash(admin.Password),
            UserRoles = [new UserRole { RoleId = adminRole.Id }],
        };

        context.Set<User>().Add(user);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded initial admin user '{UserName}'.", admin.UserName);
    }
}
