using ErpBackend.CrossCutting.Helpers;
using ErpBackend.CrossCutting.Security;
using ErpBackend.Identity.Entities;
using ErpBackend.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ErpBackend.Api.Seeding;

/// <summary>
/// DEMO-ONLY seeder (lives in the API, not in the reusable Identity package). Adds a read-only
/// "viewer" user with the ReadOnly role so the demo shows permission differences (e.g. 403 on a
/// create). Idempotent; runs after the platform identity seeder (Order 0). Password comes from
/// configuration (<c>Identity:DemoViewerPassword</c>); skipped when not set.
/// </summary>
public sealed class DemoIdentitySeeder(DbContext context, IConfiguration configuration) : IErpDataSeeder
{
    public int Order => 10;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var password = configuration["Identity:DemoViewerPassword"];
        if (string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        const string userName = "viewer";
        if (await context.Set<User>().IgnoreQueryFilters().AnyAsync(u => u.UserName == userName, cancellationToken))
        {
            return;
        }

        var readOnly = await context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name == RoleConstants.ReadOnly, cancellationToken);
        if (readOnly is null)
        {
            return;
        }

        context.Set<User>().Add(new User
        {
            UserName = userName,
            Email = "viewer@erp.local",
            DisplayName = "Demo Viewer",
            PasswordHash = PasswordHelper.Hash(password),
            UserRoles = [new UserRole { RoleId = readOnly.Id }],
        });
        await context.SaveChangesAsync(cancellationToken);
    }
}
