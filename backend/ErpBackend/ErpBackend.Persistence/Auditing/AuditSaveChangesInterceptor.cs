using ErpBackend.CrossCutting.Common;
using ErpBackend.CrossCutting.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ErpBackend.Persistence.Auditing;

/// <summary>
/// Centralizes audit stamping and soft-delete on every save, so generated repositories and
/// controllers never have to set timestamps or handle logical deletion by hand:
/// <list type="bullet">
/// <item>Added: stamps <c>CreatedAt</c> (UTC) and, for <see cref="AuditableEntity"/>, <c>CreatedBy</c>.</item>
/// <item>Modified: stamps <c>UpdatedAt</c> (UTC) and, for <see cref="AuditableEntity"/>, <c>UpdatedBy</c>.</item>
/// <item>Deleted + <see cref="SoftDeleteEntity"/>: converted to a logical delete
/// (<c>IsDeleted</c>/<c>DeletedAt</c>/<c>DeletedBy</c>) instead of a physical row removal.</item>
/// </list>
/// Registered as scoped and attached per-DbContext so it can read the current user.
/// </summary>
public sealed class AuditSaveChangesInterceptor(ICurrentUserService currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var user = currentUser.UserId ?? currentUser.UserName;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    if (entry.Entity is AuditableEntity created)
                    {
                        created.CreatedBy = user;
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    if (entry.Entity is AuditableEntity updated)
                    {
                        updated.UpdatedBy = user;
                    }
                    break;

                case EntityState.Deleted when entry.Entity is SoftDeleteEntity soft:
                    // Convert physical delete into a logical one.
                    entry.State = EntityState.Modified;
                    soft.IsDeleted = true;
                    soft.DeletedAt = now;
                    soft.DeletedBy = user;
                    soft.UpdatedAt = now;
                    break;
            }
        }
    }
}
