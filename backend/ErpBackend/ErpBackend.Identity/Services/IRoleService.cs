using ErpBackend.CrossCutting.Pagination;
using ErpBackend.Identity.Dtos;

namespace ErpBackend.Identity.Services;

/// <summary>Role management (CRUD + permission editing) over the persisted identity store.</summary>
public interface IRoleService
{
    Task<PagedResult<RoleDto>> ListAsync(RoleFilter filter, CancellationToken cancellationToken = default);
    Task<RoleDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The catalog of assignable permission keys (platform + app-declared module permissions).</summary>
    string[] AvailablePermissions();
}
