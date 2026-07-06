namespace ErpBackend.CrossCutting.Common;

/// <summary>
/// Base DTO for response models. Exposes the common read-only metadata of an entity.
/// </summary>
public abstract class BaseDto
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }
}
