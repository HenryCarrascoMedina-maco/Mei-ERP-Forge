using ErpBackend.CrossCutting.Common;

namespace ErpBackend.Persistence.Abstractions;

/// <summary>
/// Async persistence contract for an aggregate/entity. The generated per-module
/// <c>I{Name}Repository</c> interfaces extend this, and <see cref="EfRepository{TEntity}"/>
/// provides the EF Core implementation. <see cref="Query"/> returns a composable, deferred
/// <see cref="IQueryable{T}"/> so callers can add filtering/sorting before paginating with
/// <c>ToPagedResultAsync</c>.
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>Composable, deferred query (no database round-trip until materialized).</summary>
    IQueryable<TEntity> Query();

    /// <summary>Loads a single entity by id, or <c>null</c> when not found.</summary>
    Task<TEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Inserts the entity and persists the change.</summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>Updates the entity and persists the change.</summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the entity by id and persists. For <see cref="SoftDeleteEntity"/> types the
    /// delete is converted to a logical delete by the audit interceptor. Returns <c>false</c>
    /// when no entity matched.
    /// </summary>
    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
