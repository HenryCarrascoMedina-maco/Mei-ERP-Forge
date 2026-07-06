using ErpBackend.CrossCutting.Pagination;
using ErpBackend.Identity.Dtos;

namespace ErpBackend.Identity.Services;

/// <summary>User management (CRUD + role assignment) over the persisted identity store.</summary>
public interface IUserService
{
    Task<PagedResult<UserDto>> ListAsync(UserFilter filter, CancellationToken cancellationToken = default);
    Task<UserDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
