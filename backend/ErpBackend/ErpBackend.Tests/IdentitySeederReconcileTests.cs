using ErpBackend.CrossCutting.Security;
using ErpBackend.Identity.Configuration;
using ErpBackend.Identity.Entities;
using ErpBackend.Identity.Seeding;
using ErpBackend.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ErpBackend.Tests;

/// <summary>Minimal concrete context (identity model only) for exercising the seeder on EF InMemory.</summary>
file sealed class TestIdentityDbContext(DbContextOptions<TestIdentityDbContext> options) : ErpDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyErpIdentity();
        base.OnModelCreating(modelBuilder);
    }
}

public class IdentitySeederReconcileTests
{
    private static ErpDbContext NewContext() =>
        new TestIdentityDbContext(new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase("identity-" + Guid.NewGuid().ToString("N"))
            .Options);

    private static DefaultIdentitySeeder Seeder(DbContext ctx, params string[] additional) =>
        new(ctx,
            Options.Create(new ErpIdentityOptions { AdditionalAdminPermissions = additional }),
            NullLogger<DefaultIdentitySeeder>.Instance);

    [Fact]
    public async Task Reconcile_GrantsNewPermissions_ToExistingAdmin_WithoutLosingOrDuplicating()
    {
        using var ctx = NewContext();
        ctx.Set<Role>().Add(new Role { Name = RoleConstants.Administrator, Permissions = ["customers.view"] });
        await ctx.SaveChangesAsync();

        // "customers.view" already present; "widgets.view" is newly declared.
        await Seeder(ctx, "customers.view", "widgets.view").SeedAsync();

        var admin = await ctx.Set<Role>().SingleAsync(r => r.Name == RoleConstants.Administrator);
        Assert.Contains("widgets.view", admin.Permissions);                       // granted
        Assert.Contains("customers.view", admin.Permissions);                     // kept
        Assert.Equal(1, admin.Permissions.Count(p => p == "customers.view"));     // not duplicated
    }

    [Fact]
    public async Task Reconcile_IsIdempotent_AcrossRepeatedRuns()
    {
        using var ctx = NewContext();
        ctx.Set<Role>().Add(new Role { Name = RoleConstants.Administrator, Permissions = ["a"] });
        await ctx.SaveChangesAsync();

        await Seeder(ctx, "widgets.view").SeedAsync();
        var afterFirst = (await ctx.Set<Role>().SingleAsync(r => r.Name == RoleConstants.Administrator)).Permissions.Count;
        await Seeder(ctx, "widgets.view").SeedAsync();
        var afterSecond = (await ctx.Set<Role>().SingleAsync(r => r.Name == RoleConstants.Administrator)).Permissions.Count;

        Assert.Equal(afterFirst, afterSecond);
    }

    [Fact]
    public async Task Reconcile_DoesNotTouchCustomRoles()
    {
        using var ctx = NewContext();
        ctx.Set<Role>().Add(new Role { Name = RoleConstants.Administrator, Permissions = ["a"] });
        ctx.Set<Role>().Add(new Role { Name = "Warehouse", Permissions = ["stock.view"] });
        await ctx.SaveChangesAsync();

        await Seeder(ctx, "widgets.view").SeedAsync();

        var custom = await ctx.Set<Role>().SingleAsync(r => r.Name == "Warehouse");
        Assert.Equal(["stock.view"], custom.Permissions);
    }

    [Fact]
    public async Task FirstRun_SeedsBaseRoles_WhenNoneExist()
    {
        using var ctx = NewContext();

        await Seeder(ctx).SeedAsync();

        Assert.True(await ctx.Set<Role>().AnyAsync(r => r.Name == RoleConstants.Administrator));
    }
}
