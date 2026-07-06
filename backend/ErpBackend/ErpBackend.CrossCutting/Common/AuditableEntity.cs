namespace ErpBackend.CrossCutting.Common;

/// <summary>
/// Entity that tracks which user created, updated or deleted the record.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public string? DeletedBy { get; set; }
}
