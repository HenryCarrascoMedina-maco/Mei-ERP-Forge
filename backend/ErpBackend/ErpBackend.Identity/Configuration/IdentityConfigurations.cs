using ErpBackend.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpBackend.Identity.Configuration;

/// <summary>EF Core mapping for <see cref="User"/>.</summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.Property(u => u.UserName).HasMaxLength(64).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(128);
        builder.HasIndex(u => u.UserName).IsUnique();
        builder.HasMany(u => u.UserRoles).WithOne(ur => ur.User!).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF Core mapping for <see cref="Role"/>. Permissions map to a JSON column (primitive collection).</summary>
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.Property(r => r.Name).HasMaxLength(64).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(256);
        builder.HasIndex(r => r.Name).IsUnique();
        // Primitive string collection -> JSON column (EF Core 8+).
        builder.PrimitiveCollection(r => r.Permissions);
        builder.HasMany(r => r.UserRoles).WithOne(ur => ur.Role!).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>EF Core mapping for <see cref="UserRole"/> (relational join).</summary>
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
    }
}

/// <summary>EF Core mapping for <see cref="RefreshToken"/>.</summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(t => t.TokenHash);
        builder.HasIndex(t => t.UserId);
    }
}
