using ErpPlatform.Cli;
using Xunit;

namespace ErpBackend.Tests;

public class GeneratorTests
{
    private static Manifest SampleManifest() => new()
    {
        Module = new ModuleInfo { Name = "Widget", Key = "widgets", PluralName = "Widgets" },
        Entity = new EntityInfo
        {
            Base = "auditable",
            Fields =
            [
                new FieldInfo { Name = "code", Type = "text", Label = "Code", Required = true },
                new FieldInfo { Name = "price", Type = "currency", Label = "Price" },
            ],
        },
    };

    [Fact]
    public void Generate_IsDeterministic_AndByteStable_AcrossRuns()
    {
        var m = SampleManifest();
        var dirA = NewTempDir();
        var dirB = NewTempDir();
        try
        {
            Generator.Generate(m, dirA, "Test.App", "Modules");
            Generator.Generate(m, dirB, "Test.App", "Modules");

            var filesA = Directory.GetFiles(Path.Combine(dirA, "Modules", "Widget")).OrderBy(Path.GetFileName).ToList();
            var filesB = Directory.GetFiles(Path.Combine(dirB, "Modules", "Widget")).OrderBy(Path.GetFileName).ToList();

            Assert.Equal(filesA.Select(Path.GetFileName), filesB.Select(Path.GetFileName));
            foreach (var (a, b) in filesA.Zip(filesB))
            {
                Assert.Equal(File.ReadAllText(a), File.ReadAllText(b));
                // C1: emitted with LF, never CRLF (so regeneration shows no phantom diff).
                Assert.DoesNotContain("\r\n", File.ReadAllText(a));
            }
        }
        finally
        {
            Directory.Delete(dirA, recursive: true);
            Directory.Delete(dirB, recursive: true);
        }
    }

    [Fact]
    public void Generate_ReportsRoutePermissionsAndFiles()
    {
        var dir = NewTempDir();
        try
        {
            var result = Generator.Generate(SampleManifest(), dir, "Test.App", "Modules");

            Assert.Equal("widgets", result.Route);
            Assert.Contains("widgets.create", result.Permissions);
            Assert.Contains(result.Files, f => f.File == "Widget.Generated.cs");
            // No Program.cs in a bare temp dir → the tool reports it rather than throwing.
            Assert.Equal(Generator.ProgramStatus.NoProgramFile, result.Program);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "erpgen-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }
}
