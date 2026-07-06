namespace ErpBackend.CrossCutting.Common;

/// <summary>
/// Base entity providing identity and lifecycle timestamps shared by every domain entity.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
}
