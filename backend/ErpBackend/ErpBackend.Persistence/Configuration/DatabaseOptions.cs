namespace ErpBackend.Persistence.Configuration;

/// <summary>Supported relational providers. SQLite is implemented out of the box; the others are
/// design-ready and wired either via the <c>AddErpPersistence(configure: ...)</c> lambda or a
/// future companion package.</summary>
public enum DatabaseProvider
{
    Sqlite = 0,
    SqlServer = 1,
    PostgreSql = 2,
}

/// <summary>
/// Persistence configuration, bound from the <c>"Database"</c> section. The connection string is
/// taken from <c>Database:ConnectionString</c> or, if empty, from <c>ConnectionStrings:Default</c>.
/// All values are env-overridable (e.g. <c>Database__Provider</c>, <c>ConnectionStrings__Default</c>),
/// which keeps containerized deployments (<c>docker compose up</c>) configuration-only.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Provider to use. Defaults to <see cref="DatabaseProvider.Sqlite"/>.</summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;

    /// <summary>Optional explicit connection string (overrides <c>ConnectionStrings:Default</c>).</summary>
    public string? ConnectionString { get; set; }

    /// <summary>When true, pending migrations are applied at startup (handy for demo/containers).</summary>
    public bool MigrateOnStartup { get; set; }

    /// <summary>Fallback SQLite connection used when nothing is configured (zero-config onboarding).</summary>
    public const string DefaultSqliteConnection = "Data Source=erp-platform.db";
}
