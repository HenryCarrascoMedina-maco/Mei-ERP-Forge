using ErpBackend.CrossCutting.Exceptions;
using ErpBackend.CrossCutting.Pagination;
using ErpBackend.Identity.Configuration;
using ErpBackend.Identity.Dtos;
using ErpBackend.Identity.Entities;
using ErpBackend.Identity.Seeding;
using ErpBackend.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBackend.Identity.Services;

/// <summary>EF Core implementation of <see cref="IRoleService"/>.</summary>
public sealed class RoleService(DbContext context, IOptions<ErpIdentityOptions> options) : IRoleService
{
    private readonly ErpIdentityOptions _options = options.Value;

    public async Task<PagedResult<RoleDto>> ListAsync(RoleFilter filter, CancellationToken cancellationToken = default)
    {
        var query = context.Set<Role>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(term) || (r.Description ?? string.Empty).ToLower().Contains(term));
        }
        if (filter.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == filter.IsActive.Value);
        }

        var paged = await query.ToPagedResultAsync(filter, cancellationToken);
        return new PagedResult<RoleDto>(
            paged.Items.Select(ToDto).ToList(), paged.Meta.Page, paged.Meta.PageSize, paged.Meta.TotalItems);
    }

    public async Task<RoleDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => ToDto(await FindAsync(id, cancellationToken));

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.Name.Trim();
        if (await context.Set<Role>().AnyAsync(r => r.Name.ToLower() == name.ToLower(), cancellationToken))
        {
            throw new ConflictException($"A role named '{name}' already exists.");
        }

        var role = new Role
        {
            Name = name,
            Description = dto.Description,
            Permissions = SanitizePermissions(dto.Permissions),
        };
        context.Set<Role>().Add(role);
        await context.SaveChangesAsync(cancellationToken);
        return ToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await FindAsync(id, cancellationToken);
        var name = dto.Name.Trim();

        if (await context.Set<Role>().AnyAsync(r => r.Id != id && r.Name.ToLower() == name.ToLower(), cancellationToken))
        {
            throw new ConflictException($"A role named '{name}' already exists.");
        }

        role.Name = name;
        role.Description = dto.Description;
        role.Permissions = SanitizePermissions(dto.Permissions);

        await context.SaveChangesAsync(cancellationToken);
        return ToDto(role);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await FindAsync(id, cancellationToken);
        var inUse = await context.Set<UserRole>().AnyAsync(ur => ur.RoleId == id, cancellationToken);
        if (inUse)
        {
            throw new ConflictException("Role is assigned to one or more users and cannot be deleted.");
        }
        context.Set<Role>().Remove(role);
        await context.SaveChangesAsync(cancellationToken);
    }

    public string[] AvailablePermissions()
        => PermissionCatalog.Platform
            .Concat(_options.AdditionalAdminPermissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private List<string> SanitizePermissions(string[] requested)
    {
        var allowed = AvailablePermissions().ToHashSet(StringComparer.OrdinalIgnoreCase);
        return requested.Distinct(StringComparer.OrdinalIgnoreCase).Where(allowed.Contains).ToList();
    }

    private async Task<Role> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await context.Set<Role>().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        return role ?? throw new NotFoundException("Role", id);
    }

    private static RoleDto ToDto(Role r) => new()
    {
        Id = r.Id,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        IsActive = r.IsActive,
        Name = r.Name,
        Description = r.Description,
        Permissions = r.Permissions.ToArray(),
    };
}
