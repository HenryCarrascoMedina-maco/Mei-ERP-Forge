using System.Linq.Expressions;
using ErpBackend.CrossCutting.Common;
using Microsoft.EntityFrameworkCore;

namespace ErpBackend.Persistence;

/// <summary>
/// Base <see cref="DbContext"/> for ERP Platform applications. Concrete contexts (e.g.
/// <c>AppDbContext</c>) inherit this and call <see cref="ApplyErpConventions"/> from
/// <see cref="OnModelCreating"/>. It provides, provider-agnostically:
/// <list type="bullet">
/// <item>a global query filter that hides soft-deleted rows (<see cref="SoftDeleteEntity"/>);</item>
/// <item>a default precision for <c>decimal</c> columns (18,2) so no provider warnings appear.</item>
/// </list>
/// Audit stamping and soft-delete conversion are handled by the audit interceptor wired in
/// <c>AddErpPersistence</c>, not here. No SQLite/SQL Server/PostgreSQL specifics live in this class.
/// </summary>
public abstract class ErpDbContext : DbContext
{
    protected ErpDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>Default scale/precision for all <c>decimal</c> properties (overridable per entity config).</summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyErpConventions(modelBuilder);
    }

    /// <summary>
    /// Applies platform-wide model conventions. Call this from a derived context's
    /// <c>OnModelCreating</c> (after <c>ApplyConfigurationsFromAssembly</c>).
    /// </summary>
    protected static void ApplyErpConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(BuildIsNotDeletedFilter(entityType.ClrType));
            }
        }
    }

    // Builds: (TEntity e) => !e.IsDeleted  — as an untyped LambdaExpression for HasQueryFilter.
    private static LambdaExpression BuildIsNotDeletedFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");
        var isDeleted = Expression.Property(parameter, nameof(SoftDeleteEntity.IsDeleted));
        var notDeleted = Expression.Not(isDeleted);
        return Expression.Lambda(notDeleted, parameter);
    }
}
