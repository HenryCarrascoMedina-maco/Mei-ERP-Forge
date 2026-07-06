using ErpBackend.CrossCutting.Common;

namespace ErpBackend.Identity.Entities;

/// <summary>
/// Relational join between <see cref="User"/> and <see cref="Role"/>. Being a first-class auditable
/// entity (not a JSON array) makes role assignments queryable, auditable, and tenant-scopable later.
/// </summary>
public class UserRole : AuditableEntity
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public User? User { get; set; }

    public Role? Role { get; set; }
}
