using ErpBackend.CrossCutting.Responses;
using ErpBackend.CrossCutting.Security;
using ErpBackend.Identity.Dtos;
using ErpBackend.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBackend.Identity.Controllers;

/// <summary>
/// Real authentication endpoints backed by the database. Keeps <c>POST /api/auth/token</c> as the
/// login endpoint (back-compat) and adds refresh, logout and me. Authorization elsewhere is unchanged
/// (the issued JWT still carries role + permission claims).
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Validates credentials and returns a signed JWT + a refresh token.</summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Token([FromBody] LoginRequest request)
        => Ok(ApiResponse<AuthResponse>.Ok(await auth.LoginAsync(request)));

    /// <summary>Rotates a refresh token, returning a new access + refresh pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromBody] RefreshRequest request)
        => Ok(ApiResponse<AuthResponse>.Ok(await auth.RefreshAsync(request.RefreshToken)));

    /// <summary>Revokes the supplied refresh token (logout).</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutRequest request)
    {
        await auth.LogoutAsync(request.RefreshToken);
        return Ok(ApiResponse<object>.Ok(new { message = "Logged out." }));
    }

    /// <summary>Returns the authenticated user's identity, roles and permissions (from claims).</summary>
    [HttpGet("me")]
    [Authorize]
    public ActionResult<ApiResponse<object>> Me([FromServices] ICurrentUserService currentUser)
        => Ok(ApiResponse<object>.Ok(new
        {
            currentUser.UserId,
            currentUser.Email,
            currentUser.UserName,
            currentUser.Roles,
            currentUser.Permissions,
        }));
}
