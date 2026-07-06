using ErpBackend.CrossCutting.Exceptions;
using ErpBackend.CrossCutting.Helpers;
using ErpBackend.CrossCutting.Pagination;
using ErpBackend.Identity.Dtos;
using ErpBackend.Identity.Entities;
using ErpBackend.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ErpBackend.Identity.Services;

/// <summary>EF Core implementation of <see cref="IUserService"/>.</summary>
public sealed class UserService(DbContext context) : IUserService
{
    public async Task<PagedResult<UserDto>> ListAsync(UserFilter filter, CancellationToken cancellationToken = default)
    {
        var query = context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(u =>
                u.UserName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                (u.DisplayName ?? string.Empty).ToLower().Contains(term));
        }
        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }
        if (filter.RoleId.HasValue)
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == filter.RoleId.Value));
        }

        var paged = await query.ToPagedResultAsync(filter, cancellationToken);
        return new PagedResult<UserDto>(
            paged.Items.Select(ToDto).ToList(), paged.Meta.Page, paged.Meta.PageSize, paged.Meta.TotalItems);
    }

    public async Task<UserDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => ToDto(await FindAsync(id, cancellationToken));

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var userName = dto.UserName.Trim();
        var exists = await context.Set<User>().IgnoreQueryFilters()
            .AnyAsync(u => u.UserName.ToLower() == userName.ToLower(), cancellationToken);
        if (exists)
        {
            throw new ConflictException($"A user named '{userName}' already exists.");
        }

        await EnsureRolesExistAsync(dto.RoleIds, cancellationToken);

        var user = new User
        {
            UserName = userName,
            Email = dto.Email.Trim(),
            DisplayName = dto.DisplayName,
            PasswordHash = PasswordHelper.Hash(dto.Password),
            UserRoles = dto.RoleIds.Distinct().Select(roleId => new UserRole { RoleId = roleId }).ToList(),
        };

        context.Set<User>().Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return await GetAsync(user.Id, cancellationToken);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(id, cancellationToken);

        user.Email = dto.Email.Trim();
        user.DisplayName = dto.DisplayName;
        user.IsActive = dto.IsActive;
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = PasswordHelper.Hash(dto.Password);
        }

        await EnsureRolesExistAsync(dto.RoleIds, cancellationToken);
        await ReconcileRolesAsync(user, dto.RoleIds, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return await GetAsync(user.Id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(id, cancellationToken);
        context.Set<User>().Remove(user); // soft-deleted by the audit interceptor
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        return user ?? throw new NotFoundException("User", id);
    }

    private async Task EnsureRolesExistAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
        {
            return;
        }
        var distinct = roleIds.Distinct().ToList();
        var found = await context.Set<Role>().CountAsync(r => distinct.Contains(r.Id), cancellationToken);
        if (found != distinct.Count)
        {
            throw new BusinessException("One or more roles do not exist.", "ROLE_NOT_FOUND");
        }
    }

    private async Task ReconcileRolesAsync(User user, Guid[] roleIds, CancellationToken cancellationToken)
    {
        var target = roleIds.Distinct().ToHashSet();
        var current = user.UserRoles.ToList();

        var toRemove = current.Where(ur => !target.Contains(ur.RoleId)).ToList();
        if (toRemove.Count > 0)
        {
            context.Set<UserRole>().RemoveRange(toRemove);
        }

        var currentIds = current.Select(ur => ur.RoleId).ToHashSet();
        foreach (var roleId in target.Where(id => !currentIds.Contains(id)))
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        }

        await Task.CompletedTask;
    }

    private static UserDto ToDto(User u) => new()
    {
        Id = u.Id,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt,
        IsActive = u.IsActive,
        UserName = u.UserName,
        Email = u.Email,
        DisplayName = u.DisplayName,
        Roles = u.UserRoles.Where(ur => ur.Role is not null).Select(ur => ur.Role!.Name).ToArray(),
        RoleIds = u.UserRoles.Select(ur => ur.RoleId).ToArray(),
    };
}
