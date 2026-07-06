using Microsoft.EntityFrameworkCore;

namespace ErpBackend.Identity.Configuration;

/// <summary>Registers the identity entity mappings on an application's DbContext.</summary>
public static class IdentityModelBuilderExtensions
{
    /// <summary>
    /// Applies the User/Role/UserRole/RefreshToken configurations. Call from the application
    /// DbContext's <c>OnModelCreating</c> (before <c>base.OnModelCreating</c> so ERP conventions,
    /// e.g. the soft-delete filter on <c>User</c>, are applied afterwards).
    /// </summary>
    public static ModelBuilder ApplyErpIdentity(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        return modelBuilder;
    }
}
