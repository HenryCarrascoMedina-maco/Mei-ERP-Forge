using ErpBackend.CrossCutting.Common;

namespace ErpBackend.Identity.Entities;

/// <summary>
/// A named role grouping a set of permission keys. Permissions are stored as a primitive string
/// collection (JSON column) because they are a code-defined catalog (see <c>PermissionConstants</c>
/// and per-module generated permissions), not user-authored entities. (D3 hybrid decision.)
/// </summary>
public class Role : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Permission keys granted by this role, e.g. "users.view". Mapped to a JSON column.</summary>
    public List<string> Permissions { get; set; } = new();

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
