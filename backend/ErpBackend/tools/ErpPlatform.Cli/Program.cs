using System.Text.Json;
using System.Xml.Linq;
using ErpPlatform.Cli;

// ════════════════════════════════════════════════════════════════════════════
//  erpgen — ERP Platform backend generator (dotnet tool)
//  Reads a <Module>.module.json manifest and generates a backend module on top
//  of ErpPlatform.CrossCutting + ErpPlatform.Persistence.
//
//  Commands:  module (default action) · validate · help
//  Design: parse → validate → (plan) → confirm → generate → summary + next steps.
// ════════════════════════════════════════════════════════════════════════════

var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    Help.PrintOverview();
    return 0;
}

var command = args[0];
if (command is not ("module" or "validate"))
{
    Console.Error.WriteLine($"Unknown command '{command}'.\n");
    Help.PrintOverview();
    return 1;
}

var rest = args.Skip(1).ToArray();
if (rest.Contains("-h") || rest.Contains("--help"))
{
    Help.PrintCommand(command);
    return 0;
}

var (positionals, flags) = ArgParser.Parse(rest);

// ---- manifest (positional or --manifest) ----------------------------------
var manifestArg = flags.GetValueOrDefault("manifest") ?? positionals.FirstOrDefault();
if (manifestArg is null)
{
    Console.Error.WriteLine($"""
        ✗ No manifest specified.

          Pass the path to a <Module>.module.json, e.g.:
            erpgen {command} ./MyModule.module.json

          The manifest shape is documented in module-manifest.schema.json
          (see the schematics/schemas folder shipped with @erp-platform/core).
        """);
    return 1;
}

var manifestPath = Path.GetFullPath(manifestArg);
if (!File.Exists(manifestPath))
{
    Console.Error.WriteLine($"✗ Manifest not found:\n    {manifestPath}\n  (current directory: {Directory.GetCurrentDirectory()})");
    return 1;
}

Manifest manifest;
try
{
    manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(manifestPath), jsonOpts) ?? new Manifest();
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"✗ Could not parse the manifest as JSON:\n    {manifestPath}\n    {ex.Message}");
    return 1;
}

// ---- validate (both commands) ---------------------------------------------
var problems = ManifestValidator.Validate(manifest);
if (problems.Count > 0)
{
    Console.Error.WriteLine($"✗ {manifest.Module.Name ?? "Manifest"} has {problems.Count} problem(s):\n");
    foreach (var p in problems)
    {
        Console.Error.WriteLine($"    • {p}");
    }
    Console.Error.WriteLine("\n  Fix these and re-run. (erpgen validate <manifest> checks without generating.)");
    return 1;
}

if (command == "validate")
{
    Console.WriteLine($"✓ Manifest is valid — {manifest.Module.Name} · {manifest.Entity.Fields.Count} field(s) · key '{manifest.Module.Key}'.");
    return 0;
}

// ════════════════════════════ module command ═══════════════════════════════

// ---- store (ef default | inmemory) ----------------------------------------
var store = Generator.Store.Ef;
if (flags.TryGetValue("store", out var storeValue))
{
    switch (storeValue.ToLowerInvariant())
    {
        case "ef":
            store = Generator.Store.Ef;
            break;
        case "inmemory" or "in-memory":
            store = Generator.Store.InMemory;
            break;
        default:
            Console.Error.WriteLine($"✗ Invalid --store '{storeValue}'. Use 'ef' (default) or 'inmemory'.");
            return 1;
    }
}

// ---- target project (explicit --project, else auto-detect) ----------------
string projectDir;
if (flags.TryGetValue("project", out var projectFlag))
{
    projectDir = Path.GetFullPath(projectFlag);
}
else
{
    var detected = ProjectLocator.FindApiProject(Directory.GetCurrentDirectory());
    if (detected is null)
    {
        Console.Error.WriteLine($"""
            ✗ Could not locate the target API project automatically.

              erpgen looks for a folder containing Program.cs with the anchor
              '{Generator.ProgramAnchor}'. Pass it explicitly:
                erpgen module {manifestArg} --project path/to/YourApi
            """);
        return 1;
    }
    projectDir = detected;
    Console.WriteLine($"• Target project (auto-detected): {ArgParser.Rel(projectDir)}");
}

if (!Directory.Exists(projectDir))
{
    Console.Error.WriteLine($"✗ Project directory not found: {projectDir}");
    return 1;
}

// ---- namespace (explicit --namespace, else from the .csproj) --------------
var pathSegment = flags.GetValueOrDefault("path") ?? "Modules";
string rootNamespace;
if (flags.TryGetValue("namespace", out var nsFlag))
{
    rootNamespace = nsFlag;
}
else if (ProjectLocator.DetectRootNamespace(projectDir) is { } detectedNs)
{
    rootNamespace = detectedNs;
    Console.WriteLine($"• Root namespace (auto-detected): {rootNamespace}");
}
else
{
    rootNamespace = "ErpApp";
    Console.WriteLine("⚠ Could not detect the root namespace; using 'ErpApp'. Pass --namespace to override.");
}

var dryRun = flags.ContainsKey("dry-run") || flags.ContainsKey("dryrun");
var assumeYes = flags.ContainsKey("yes") || flags.ContainsKey("y") || flags.ContainsKey("force");

// Plan first (writes nothing) — powers the summary and the overwrite prompt.
var plan = Generator.Generate(manifest, projectDir, rootNamespace, pathSegment, store, dryRun: true);

if (dryRun)
{
    Summary.Print(plan, projectDir, dryRun: true);
    return 0;
}

// Overwrite awareness: only prompt in an interactive terminal (CI/piped runs proceed).
var overwrites = plan.Files.Where(f => f.Status == Generator.FileStatus.Overwritten).ToList();
if (overwrites.Count > 0 && !assumeYes && !Console.IsInputRedirected)
{
    Console.WriteLine($"⚠ This will overwrite {overwrites.Count} generated file(s) in {ArgParser.Rel(plan.Directory)}:");
    foreach (var f in overwrites)
    {
        Console.WriteLine($"    ~ {f.File}");
    }
    Console.Write("Continue? [y/N] ");
    var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
    if (answer is not ("y" or "yes"))
    {
        Console.WriteLine("Aborted — nothing was written.");
        return 0;
    }
}

var result = Generator.Generate(manifest, projectDir, rootNamespace, pathSegment, store, dryRun: false);
Summary.Print(result, projectDir, dryRun: false);
return 0;

// ════════════════════════════════ helpers ══════════════════════════════════

/// <summary>Splits raw args into positionals and flags. Supports --key value, --key=value,
/// boolean flags (--dry-run) and short -y. Boolean flags map to an empty string value.</summary>
static class ArgParser
{
    public static (List<string> Positionals, Dictionary<string, string> Flags) Parse(IEnumerable<string> args)
    {
        var positionals = new List<string>();
        var flags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var tokens = args.ToList();

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("--", StringComparison.Ordinal))
            {
                var body = token[2..];
                var eq = body.IndexOf('=');
                if (eq >= 0)
                {
                    flags[body[..eq]] = body[(eq + 1)..];
                }
                else if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith('-'))
                {
                    flags[body] = tokens[++i];
                }
                else
                {
                    flags[body] = string.Empty; // boolean flag
                }
            }
            else if (token.StartsWith('-') && token.Length > 1)
            {
                flags[token[1..]] = string.Empty; // short boolean flag, e.g. -y
            }
            else
            {
                positionals.Add(token);
            }
        }

        return (positionals, flags);
    }

    /// <summary>Path relative to the current directory (for tidy display); falls back to the full path.</summary>
    public static string Rel(string path)
    {
        try
        {
            var rel = Path.GetRelativePath(Directory.GetCurrentDirectory(), path);
            return rel.StartsWith("..", StringComparison.Ordinal) ? path : rel;
        }
        catch
        {
            return path;
        }
    }
}

/// <summary>Finds the target API project and its root namespace.</summary>
static class ProjectLocator
{
    private static readonly string[] SkipDirs = ["bin", "obj", "node_modules", ".git", ".vs"];

    /// <summary>Searches <paramref name="startDir"/> downward for a Program.cs that contains the
    /// module anchor, and returns its folder. Null if none is found.</summary>
    public static string? FindApiProject(string startDir)
    {
        foreach (var program in EnumerateProgramFiles(startDir))
        {
            try
            {
                if (File.ReadAllText(program).Contains(Generator.ProgramAnchor, StringComparison.Ordinal))
                {
                    return Path.GetDirectoryName(program);
                }
            }
            catch
            {
                // Unreadable file — skip.
            }
        }
        return null;
    }

    /// <summary>Reads &lt;RootNamespace&gt; from the project's .csproj, else the .csproj base name
    /// (e.g. ErpBackend.Api.csproj → "ErpBackend.Api"). Null if no .csproj is present.</summary>
    public static string? DetectRootNamespace(string projectDir)
    {
        var csproj = Directory.EnumerateFiles(projectDir, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (csproj is null)
        {
            return null;
        }

        try
        {
            var rootNs = XDocument.Load(csproj).Descendants("RootNamespace").FirstOrDefault()?.Value;
            if (!string.IsNullOrWhiteSpace(rootNs))
            {
                return rootNs.Trim();
            }
        }
        catch
        {
            // Malformed csproj — fall back to the file name.
        }

        return Path.GetFileNameWithoutExtension(csproj);
    }

    private static IEnumerable<string> EnumerateProgramFiles(string root)
    {
        var stack = new Stack<string>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var dir = stack.Pop();
            string[] files;
            try
            {
                files = Directory.GetFiles(dir, "Program.cs", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                continue;
            }

            foreach (var f in files)
            {
                yield return f;
            }

            IEnumerable<string> subdirs;
            try
            {
                subdirs = Directory.EnumerateDirectories(dir);
            }
            catch
            {
                continue;
            }

            foreach (var sub in subdirs)
            {
                var name = Path.GetFileName(sub);
                if (!SkipDirs.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    stack.Push(sub);
                }
            }
        }
    }
}

/// <summary>Renders the post-generation summary and the ordered next steps.</summary>
static class Summary
{
    public static void Print(Generator.GenerateResult r, string projectDir, bool dryRun)
    {
        var storeLabel = r.Store == Generator.Store.Ef ? "EF Core" : "in-memory";
        Console.WriteLine();
        Console.WriteLine(dryRun
            ? $"DRY RUN — module '{r.Module}' ({storeLabel}). No files written."
            : $"✓ Module '{r.Module}' generated ({storeLabel}).");
        Console.WriteLine($"    Namespace : {r.Namespace}");
        Console.WriteLine($"    Location  : {ArgParser.Rel(r.Directory)}");
        Console.WriteLine($"    API route : /api/{r.Route}");

        Console.WriteLine();
        Console.WriteLine("  Files:");
        foreach (var f in r.Files)
        {
            var (mark, label) = f.Status switch
            {
                Generator.FileStatus.Created => ("+", dryRun ? "would create" : "created"),
                Generator.FileStatus.Overwritten => ("~", dryRun ? "would overwrite" : "overwritten"),
                _ => ("·", "kept"),
            };
            Console.WriteLine($"    {mark} {f.File,-42} ({label})");
        }

        Console.WriteLine();
        Console.Write("  Program.cs: ");
        Console.WriteLine(r.Program switch
        {
            Generator.ProgramStatus.Patched => dryRun
                ? $"would register builder.Services.Add{r.Module}Module();"
                : $"registered builder.Services.Add{r.Module}Module();",
            Generator.ProgramStatus.AlreadyRegistered => $"already registers Add{r.Module}Module() (unchanged).",
            Generator.ProgramStatus.AnchorMissing =>
                $"⚠ anchor '{Generator.ProgramAnchor}' not found — add this line manually:\n"
                + $"                builder.Services.Add{r.Module}Module();",
            _ => "⚠ no Program.cs in the target project — register the module in your composition root:\n"
                + $"                builder.Services.Add{r.Module}Module();",
        });

        Console.WriteLine();
        Console.WriteLine("  Permissions declared (guard endpoints / seed to roles):");
        Console.WriteLine($"    {string.Join(", ", r.Permissions)}");

        Console.WriteLine();
        Console.WriteLine("  Next steps:");
        var step = 1;
        if (r.Store == Generator.Store.Ef)
        {
            Console.WriteLine($"    {step++}. Create the EF migration for the new entity:");
            Console.WriteLine($"         dotnet ef migrations add Add{r.Module} --project {ArgParser.Rel(projectDir)}");
        }
        Console.WriteLine($"    {step++}. Grant the permissions to a role (otherwise endpoints return 403):");
        Console.WriteLine($"         add them to Identity:AdditionalAdminPermissions and restart (the admin");
        Console.WriteLine($"         role auto-reconciles), or assign them via the Roles admin screen.");
        Console.WriteLine($"    {step++}. Run the API:  dotnet run --project {ArgParser.Rel(projectDir)}");
        Console.WriteLine();
    }
}

/// <summary>Help text: an overview and per-command detail with examples.</summary>
static class Help
{
    public static void PrintOverview()
    {
        Console.WriteLine("""
            erpgen — ERP Platform backend generator

            Usage:
              erpgen <command> [manifest] [options]

            Commands:
              module     Generate a backend module from a manifest (default action).
              validate   Check a manifest against the schema rules — generates nothing.
              help       Show this help.  (also: -h, --help)

            Run 'erpgen module --help' or 'erpgen validate --help' for details and examples.

            Generates entity, DTOs + validation, permissions, repository, async controller and DI
            on top of ErpPlatform.CrossCutting + ErpPlatform.Persistence.
            """);
    }

    public static void PrintCommand(string command)
    {
        if (command == "validate")
        {
            Console.WriteLine("""
                erpgen validate — validate a module manifest without generating code.

                Usage:
                  erpgen validate <manifest>

                Example:
                  erpgen validate ./Customer.module.json

                Reports PascalCase/kebab-case, reserved/duplicate field names, unsupported types
                and missing select options — the same rules the generator enforces.
                """);
            return;
        }

        Console.WriteLine($"""
            erpgen module — generate a backend module from a manifest.

            Usage:
              erpgen module <manifest> [options]

            Options:
              --manifest <path>     Manifest path (or pass it as the first argument).
              --project <dir>       Target project (folder with Program.cs). Auto-detected if omitted.
              --namespace <ns>      Root namespace. Auto-detected from the .csproj if omitted.
              --path <segment>      Folder/namespace segment under the project. Default: Modules.
              --store ef|inmemory   Backing store. Default: ef (EF Core repository).
              --dry-run             Show what would be generated; write nothing.
              --yes, -y, --force    Skip the overwrite confirmation (implied in CI / when piped).

            Examples:
              erpgen module ./Customer.module.json
              erpgen module ./Customer.module.json --dry-run
              erpgen module ./Customer.module.json --project ./src/MyApi --store inmemory

            Re-running overwrites *.Generated.cs and keeps any editable in-memory repository.
            Registers the module in Program.cs at the '{Generator.ProgramAnchor}' anchor.
            """);
    }
}
