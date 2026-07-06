using System.ComponentModel.DataAnnotations;
using ErpBackend.CrossCutting.Common;
using ErpBackend.CrossCutting.Pagination;

namespace ErpBackend.Identity.Dtos;

/// <summary>Role list filter (inherited search + isActive + pagination).</summary>
public sealed class RoleFilter : FilterParams;

/// <summary>Role read model.</summary>
public sealed class RoleDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string[] Permissions { get; set; } = [];
}

/// <summary>Create-role payload.</summary>
public sealed class CreateRoleDto
{
    [Required]
    [StringLength(64, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(256)]
    public string? Description { get; set; }

    /// <summary>Permission keys granted by this role (from the assignable catalog).</summary>
    public string[] Permissions { get; set; } = [];
}

/// <summary>Update-role payload.</summary>
public sealed class UpdateRoleDto
{
    [Required]
    [StringLength(64, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(256)]
    public string? Description { get; set; }

    public string[] Permissions { get; set; } = [];
}
