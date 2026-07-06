using ErpPlatform.Cli;
using Xunit;

namespace ErpBackend.Tests;

public class ManifestValidatorTests
{
    private static Manifest ValidManifest() => new()
    {
        Module = new ModuleInfo { Name = "Customer", Key = "customers", PluralName = "Customers" },
        Entity = new EntityInfo
        {
            Base = "auditable",
            Fields =
            [
                new FieldInfo { Name = "code", Type = "text", Label = "Code", Required = true },
                new FieldInfo { Name = "isActive", Type = "boolean", Label = "Status" }, // filtered base field: allowed
            ],
        },
    };

    [Fact]
    public void Validate_ReturnsNoErrors_ForAValidManifest()
    {
        Assert.Empty(ManifestValidator.Validate(ValidManifest()));
    }

    [Fact]
    public void Validate_FlagsAllProblems_InABadManifest()
    {
        var bad = new Manifest
        {
            Module = new ModuleInfo { Name = "bad name", Key = "Bad_Key" },
            Entity = new EntityInfo
            {
                Base = "weird",
                Fields =
                [
                    new FieldInfo { Name = "Total Price", Type = "money", Label = "Total" },
                    new FieldInfo { Name = "createdBy", Type = "text", Label = "By" },
                    new FieldInfo { Name = "seg", Type = "select", Label = "Segment" }, // no options
                    new FieldInfo { Name = "seg", Type = "text", Label = "Dup" },        // duplicate
                ],
            },
        };

        var errors = ManifestValidator.Validate(bad);
        var joined = string.Join("\n", errors);

        Assert.Contains("module.name", joined);
        Assert.Contains("module.key", joined);
        Assert.Contains("entity.base", joined);
        Assert.Contains("camelCase", joined);          // "Total Price"
        Assert.Contains("unsupported type 'money'", joined);
        Assert.Contains("automatically", joined);       // reserved createdBy
        Assert.Contains("at least one option", joined); // select without options
        Assert.Contains("duplicate", joined);
    }

    [Fact]
    public void Validate_RequiresAtLeastOneField()
    {
        var m = ValidManifest();
        m.Entity.Fields.Clear();
        Assert.Contains(ManifestValidator.Validate(m), e => e.Contains("at least one field"));
    }
}
