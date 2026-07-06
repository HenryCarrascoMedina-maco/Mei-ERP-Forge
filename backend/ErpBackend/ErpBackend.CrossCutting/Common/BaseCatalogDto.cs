namespace ErpBackend.CrossCutting.Common;

/// <summary>
/// Standard DTO for catalog entities (lookup tables) exposing a code/name pair.
/// </summary>
public class BaseCatalogDto : BaseDto
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
