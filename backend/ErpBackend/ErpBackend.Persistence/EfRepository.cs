using ErpBackend.CrossCutting.Common;
using ErpBackend.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ErpBackend.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IRepository{TEntity}"/>. Generated per-module
/// repositories inherit this (<c>{Name}EfRepository : EfRepository&lt;{Name}&gt;</c>) so they are
/// just a constructor. Persists each mutation immediately; audit stamping and soft-delete are
/// applied transparently by the audit interceptor.
/// </summary>
public class EfRepository<TEntity>(DbContext context) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>The underlying EF Core context (the application's <c>AppDbContext</c>).</summary>
    protected DbContext Context { get; } = context;

    /// <summary>The tracked set for <typeparamref name="TEntity"/>.</summary>
    protected DbSet<TEntity> Set => Context.Set<TEntity>();

    public IQueryable<TEntity> Query() => Set.AsQueryable();

    public Task<TEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Set.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        // For SoftDeleteEntity types the interceptor converts this to a logical delete.
        Set.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
