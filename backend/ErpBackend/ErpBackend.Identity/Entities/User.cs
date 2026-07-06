using ErpBackend.CrossCutting.Common;

namespace ErpBackend.Identity.Entities;

/// <summary>
/// An authenticatable user. Soft-deletable and auditable. Password is stored as a PBKDF2 hash
/// (see <c>PasswordHelper</c>). Role membership is relational via <see cref="UserRole"/>.
/// </summary>
public class User : SoftDeleteEntity
{
    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash produced by <c>PasswordHelper.Hash</c>.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    /// <summary>Role assignments (join). The effective permission set is the union of the roles' permissions.</summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
