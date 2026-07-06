namespace ErpBackend.CrossCutting.Common;

/// <summary>
/// Auditable entity that supports logical (soft) deletion instead of physical removal.
/// </summary>
public abstract class SoftDeleteEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
