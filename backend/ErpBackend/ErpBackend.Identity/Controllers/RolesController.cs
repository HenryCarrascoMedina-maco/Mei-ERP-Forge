using ErpBackend.CrossCutting.Pagination;
using ErpBackend.CrossCutting.Responses;
using ErpBackend.CrossCutting.Security;
using ErpBackend.Identity.Dtos;
using ErpBackend.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpBackend.Identity.Controllers;

/// <summary>Role management endpoints (persisted identity). Permission-gated with <c>roles.*</c>.</summary>
[ApiController]
[Route("api/security/roles")]
[Produces("application/json")]
public class RolesController(IRoleService roles) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionConstants.Roles.View)]
    public async Task<ActionResult<PagedResponse<RoleDto>>> GetAll([FromQuery] RoleFilter filter)
        => Ok(PagedResponse<RoleDto>.From(await roles.ListAsync(filter)));

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.Roles.View)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id)
        => Ok(ApiResponse<RoleDto>.Ok(await roles.GetAsync(id)));

    /// <summary>The catalog of assignable permission keys (for the role editor).</summary>
    [HttpGet("permissions")]
    [HasPermission(PermissionConstants.Roles.View)]
    public ActionResult<ApiResponse<string[]>> Permissions()
        => Ok(ApiResponse<string[]>.Ok(roles.AvailablePermissions()));

    [HttpPost]
    [HasPermission(PermissionConstants.Roles.Create)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleDto dto)
    {
        var created = await roles.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RoleDto>.Ok(created));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.Roles.Update)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(Guid id, [FromBody] UpdateRoleDto dto)
        => Ok(ApiResponse<RoleDto>.Ok(await roles.UpdateAsync(id, dto)));

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.Roles.Delete)]
    public async Task<ActionResult<NoContentResponse>> Delete(Guid id)
    {
        await roles.DeleteAsync(id);
        return Ok(new NoContentResponse());
    }
}
