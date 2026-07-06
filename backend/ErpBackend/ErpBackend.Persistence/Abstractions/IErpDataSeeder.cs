namespace ErpBackend.Persistence.Abstractions;

/// <summary>
/// A unit of initial/demo data. Seeders are discovered via DI and executed in ascending
/// <see cref="Order"/> by <c>SeedErpDataAsync</c>. Implementations MUST be idempotent
/// (check-then-insert) so they are safe to run on every startup and on a pre-populated database.
/// </summary>
public interface IErpDataSeeder
{
    /// <summary>Relative execution order (lower runs first). Use to honor dependencies,
    /// e.g. seed roles/permissions before users, and users before business data.</summary>
    int Order { get; }

    /// <summary>Seeds the data. Must be idempotent.</summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
