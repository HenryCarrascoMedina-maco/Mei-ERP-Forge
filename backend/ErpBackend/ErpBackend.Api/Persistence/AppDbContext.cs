using ErpBackend.Identity.Configuration;
using ErpBackend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpBackend.Api.Persistence;

/// <summary>
/// The application's EF Core context. Inherits <see cref="ErpDbContext"/> (soft-delete filter,
/// decimal conventions, audit interceptor) and auto-discovers each module's entity mapping via
/// <c>ApplyConfigurationsFromAssembly</c> — so generated modules require <b>no</b> changes to this
/// file (they just drop in an <c>IEntityTypeConfiguration&lt;T&gt;</c>).
/// </summary>
/// <remarks>
/// Until the generator emits entity configurations (Slice 3) and the demo modules are migrated
/// (Slice 4), the model is intentionally empty: the API still runs on the existing in-memory
/// repositories. This context is registered and ready, but not yet the source of truth.
/// </remarks>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : ErpDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.ApplyErpIdentity(); // User/Role/UserRole/RefreshToken mappings
        base.OnModelCreating(modelBuilder); // applies ERP conventions (soft-delete filter, etc.)
    }
}
