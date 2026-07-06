using ErpBackend.CrossCutting.Pagination;
using ErpBackend.CrossCutting.Responses;
using ErpBackend.CrossCutting.Security;
using ErpBackend.Identity.Dtos;
using ErpBackend.Identity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ErpBackend.Identity.Controllers;

/// <summary>User management endpoints (persisted identity). Permission-gated with <c>users.*</c>.</summary>
[ApiController]
[Route("api/security/users")]
[Produces("application/json")]
public class UsersController(IUserService users) : ControllerBase
{
    [HttpGet]
    [HasPermission(PermissionConstants.Users.View)]
    public async Task<ActionResult<PagedResponse<UserDto>>> GetAll([FromQuery] UserFilter filter)
        => Ok(PagedResponse<UserDto>.From(await users.ListAsync(filter)));

    [HttpGet("{id:guid}")]
    [HasPermission(PermissionConstants.Users.View)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id)
        => Ok(ApiResponse<UserDto>.Ok(await users.GetAsync(id)));

    [HttpPost]
    [HasPermission(PermissionConstants.Users.Create)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto)
    {
        var created = await users.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserDto>.Ok(created));
    }

    [HttpPut("{id:guid}")]
    [HasPermission(PermissionConstants.Users.Update)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(Guid id, [FromBody] UpdateUserDto dto)
        => Ok(ApiResponse<UserDto>.Ok(await users.UpdateAsync(id, dto)));

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionConstants.Users.Delete)]
    public async Task<ActionResult<NoContentResponse>> Delete(Guid id)
    {
        await users.DeleteAsync(id);
        return Ok(new NoContentResponse());
    }
}
