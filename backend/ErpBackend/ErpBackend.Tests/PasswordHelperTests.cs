using ErpBackend.CrossCutting.Helpers;
using Xunit;

namespace ErpBackend.Tests;

public class PasswordHelperTests
{
    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var hash = PasswordHelper.Hash("Admin123!");
        Assert.True(PasswordHelper.Verify("Admin123!", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hash = PasswordHelper.Hash("Admin123!");
        Assert.False(PasswordHelper.Verify("wrong", hash));
    }

    [Fact]
    public void Hash_IsSalted_SoTwoHashesOfSamePasswordDiffer()
    {
        Assert.NotEqual(PasswordHelper.Hash("same"), PasswordHelper.Hash("same"));
    }
}
