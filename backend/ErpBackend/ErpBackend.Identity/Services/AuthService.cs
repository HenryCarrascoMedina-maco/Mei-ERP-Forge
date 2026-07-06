using ErpBackend.CrossCutting.Exceptions;
using ErpBackend.CrossCutting.Helpers;
using ErpBackend.CrossCutting.Security;
using ErpBackend.Identity.Dtos;
using ErpBackend.Identity.Entities;
using ErpBackend.Identity.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBackend.Identity.Services;

/// <summary>Default <see cref="IAuthService"/> over the application DbContext.</summary>
public sealed class AuthService(DbContext context, IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        EnsureJwtConfigured();

        var user = await FindUserByNameAsync(request.UserName, cancellationToken);
        if (user is null || !user.IsActive || !PasswordHelper.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        return await IssueAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        EnsureJwtConfigured();

        var hash = TokenHasher.Hash(refreshToken);
        var stored = await context.Set<RefreshToken>().FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (stored is null || !stored.IsValid(DateTime.UtcNow))
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var user = await FindUserByIdAsync(stored.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("User is no longer active.");
        }

        // Rotation: revoke the presented token, then issue a fresh pair (saved together).
        stored.RevokedAtUtc = DateTime.UtcNow;
        return await IssueAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = TokenHasher.Hash(refreshToken);
        var stored = await context.Set<RefreshToken>().FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (stored is { RevokedAtUtc: null })
        {
            stored.RevokedAtUtc = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken)
    {
        var roles = user.UserRoles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role!.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = user.UserRoles
            .Where(ur => ur.Role is not null)
            .SelectMany(ur => ur.Role!.Permissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var claims = ClaimsHelper.BuildClaims(
            userId: user.Id.ToString(),
            email: user.Email,
            userName: user.UserName,
            roles: roles,
            permissions: permissions);

        var token = JwtHelper.GenerateToken(claims, _jwt);

        var rawRefresh = JwtHelper.GenerateRefreshToken();
        var refreshExpires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays);
        context.Set<RefreshToken>().Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawRefresh),
            ExpiresAtUtc = refreshExpires,
        });
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = token.AccessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = token.ExpiresAtUtc,
            RefreshToken = rawRefresh,
            RefreshTokenExpiresAtUtc = refreshExpires,
            Roles = roles,
            Permissions = permissions,
        };
    }

    private Task<User?> FindUserByNameAsync(string userName, CancellationToken cancellationToken)
    {
        var normalized = userName.Trim().ToLower();
        return context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalized, cancellationToken);
    }

    private Task<User?> FindUserByIdAsync(Guid id, CancellationToken cancellationToken)
        => context.Set<User>()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    private void EnsureJwtConfigured()
    {
        if (!_jwt.IsConfigured)
        {
            throw new BusinessException("JWT is not configured. Set the Jwt:SecretKey setting.", "JWT_NOT_CONFIGURED");
        }
    }
}
