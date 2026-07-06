namespace ErpBackend.CrossCutting.Common;

/// <summary>
/// Base DTO for update requests. Carries the identifier of the resource being updated.
/// </summary>
public abstract class BaseUpdateDto
{
    public Guid Id { get; set; }
}
