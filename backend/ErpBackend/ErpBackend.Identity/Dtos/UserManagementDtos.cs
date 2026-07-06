using System.ComponentModel.DataAnnotations;
using ErpBackend.CrossCutting.Common;
using ErpBackend.CrossCutting.Pagination;

namespace ErpBackend.Identity.Dtos;

/// <summary>User read model.</summary>
public sealed class UserDto : BaseDto
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    /// <summary>Role names (for display).</summary>
    public string[] Roles { get; set; } = [];
    /// <summary>Role ids (for the edit form's role selector).</summary>
    public Guid[] RoleIds { get; set; } = [];
}

/// <summary>Create-user payload (validated via AddErpApiValidation → 422).</summary>
public sealed class CreateUserDto
{
    [Required]
    [StringLength(64, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(128)]
    public string? DisplayName { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Roles to assign by id.</summary>
    public Guid[] RoleIds { get; set; } = [];
}

/// <summary>Update-user payload. <see cref="Password"/> is optional (set to reset it).</summary>
public sealed class UpdateUserDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(128)]
    public string? DisplayName { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(128, MinimumLength = 6)]
    public string? Password { get; set; }

    public Guid[] RoleIds { get; set; } = [];
}

/// <summary>User list filter: inherited search + isActive, plus an optional role filter.</summary>
public sealed class UserFilter : FilterParams
{
    public Guid? RoleId { get; set; }
}
