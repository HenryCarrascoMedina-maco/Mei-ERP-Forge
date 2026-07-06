using System.Globalization;
using System.Text;

namespace ErpPlatform.Cli;

/// <summary>Emits a backend module (regeneration-safe) from a manifest, on top of ErpPlatform.CrossCutting.</summary>
public static class Generator
{
    private static readonly HashSet<string> BaseFields =
        new(StringComparer.OrdinalIgnoreCase) { "id", "createdAt", "updatedAt", "isActive" };

    /// <summary>Backing store for the generated repository: EF Core (default) or in-memory.</summary>
    public enum Store
    {
        Ef,
        InMemory,
    }

    /// <summary>What happened to a single emitted file.</summary>
    public enum FileStatus
    {
        Created,
        Overwritten,
        Kept,
    }

    /// <summary>Outcome of the Program.cs registration patch.</summary>
    public enum ProgramStatus
    {
        Patched,
        AlreadyRegistered,
        AnchorMissing,
        NoProgramFile,
    }

    /// <summary>One emitted (or planned) file and how it was written.</summary>
    public sealed record FileOp(string File, FileStatus Status);

    /// <summary>Everything the caller needs to render a summary and next steps (no console I/O here).</summary>
    public sealed record GenerateResult(
        string Module,
        string Namespace,
        string Directory,
        string Route,
        string PermissionPrefix,
        IReadOnlyList<string> Permissions,
        IReadOnlyList<FileOp> Files,
        ProgramStatus Program,
        Store Store);

    /// <summary>
    /// Generates (or, when <paramref name="dryRun"/> is true, plans) a backend module from
    /// <paramref name="m"/> into <c>{projectDir}/{pathSegment}/{Name}</c> and registers it in the
    /// target Program.cs at the <c>// erp-generated:modules</c> anchor. Regeneration-safe: overwrites
    /// <c>*.Generated.cs</c> and keeps any editable in-memory repository. Returns a
    /// <see cref="GenerateResult"/> describing what was written; performs no console output.
    /// </summary>
    public static GenerateResult Generate(
        Manifest m, string projectDir, string rootNamespace, string pathSegment,
        Store store = Store.Ef, bool dryRun = false)
    {
        var name = m.Module.Name;
        var ns = $"{rootNamespace}.{pathSegment.Replace('/', '.')}.{name}";
        var dir = Path.Combine(projectDir, pathSegment.Replace('/', Path.DirectorySeparatorChar), name);
        if (!dryRun)
        {
            Directory.CreateDirectory(dir);
        }

        var fields = m.Entity.Fields.Where(f => !BaseFields.Contains(f.Name)).ToList();
        var ops = new List<FileOp>
        {
            Emit(dir, $"{name}.Generated.cs", BuildEntity(m, ns, fields), regen: true, dryRun),
            Emit(dir, $"{name}Dtos.Generated.cs", BuildDtos(m, ns, fields), regen: true, dryRun),
            Emit(dir, $"{name}Permissions.Generated.cs", BuildPermissions(m, ns), regen: true, dryRun),
            Emit(dir, $"I{name}Repository.Generated.cs", BuildRepositoryInterface(name, ns), regen: true, dryRun),
            Emit(dir, $"{name}Controller.Generated.cs", BuildController(m, ns, fields, store), regen: true, dryRun),
            Emit(dir, $"{name}ModuleExtensions.Generated.cs", BuildModuleExtensions(name, ns, store), regen: true, dryRun),
        };

        if (store == Store.Ef)
        {
            ops.Add(Emit(dir, $"{name}Configuration.Generated.cs", BuildEntityConfiguration(m, ns, fields), regen: true, dryRun));
            ops.Add(Emit(dir, $"{name}EfRepository.Generated.cs", BuildEfRepository(name, ns), regen: true, dryRun));
        }
        else
        {
            // Editable in-memory repository: created once, never overwritten (custom code may live here).
            ops.Add(Emit(dir, $"{name}InMemoryRepository.cs", BuildInMemoryRepository(name, ns), regen: false, dryRun));
        }

        var program = PatchProgram(projectDir, name, dryRun);
        var prefix = m.Permissions?.Prefix ?? m.Module.Key;

        return new GenerateResult(name, ns, dir, ResourceRoute(m), prefix, PermissionKeys(prefix), ops, program, store);
    }

    /// <summary>The six permission keys the generated module declares (mirrors {Name}Permissions).</summary>
    private static string[] PermissionKeys(string prefix) =>
        [$"{prefix}.view", $"{prefix}.list", $"{prefix}.create", $"{prefix}.update", $"{prefix}.delete", $"{prefix}.changeStatus"];

    // ---- emitters -----------------------------------------------------------

    private static string EntityBase(string? b) => b switch
    {
        "base" => "BaseEntity",
        "softDelete" or "catalog" => "SoftDeleteEntity",
        _ => "AuditableEntity",
    };

    private static string CsType(FieldInfo f) => f.Type switch
    {
        "number" => "int",
        "decimal" or "currency" => "decimal",
        "boolean" => "bool",
        "date" or "datetime" => "DateTime",
        "multiselect" => "List<string>",
        _ => "string",
    };

    private static string Pascal(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];

    private static string Prop(FieldInfo f)
    {
        var name = Pascal(f.Name);
        var type = CsType(f);
        if (type == "string")
            return f.Required ? $"    public string {name} {{ get; set; }} = string.Empty;" : $"    public string? {name} {{ get; set; }}";
        if (type == "List<string>")
            return $"    public List<string> {name} {{ get; set; }} = new();";
        if (type == "bool")
            return $"    public bool {name} {{ get; set; }}";
        // value types (int/decimal/DateTime)
        return f.Required ? $"    public {type} {name} {{ get; set; }}" : $"    public {type}? {name} {{ get; set; }}";
    }

    private static string BuildEntity(Manifest m, string ns, List<FieldInfo> fields)
    {
        var props = string.Join("\n", fields.Select(Prop));
        return $@"// <auto-generated>Generated from {m.Module.Name}.module.json by erpgen. DO NOT EDIT.</auto-generated>
#nullable enable
using ErpBackend.CrossCutting.Common;

namespace {ns};

public class {m.Module.Name} : {EntityBase(m.Entity.Base)}
{{
{props}
}}
";
    }

    private static string Annotations(FieldInfo f)
    {
        var a = new List<string>();
        if (f.Required) a.Add("[Required]");
        if (f.Type == "email" || f.Validation?.Email == true) a.Add("[EmailAddress]");

        var v = f.Validation;
        if (CsType(f) == "string" && v is not null)
        {
            if (v.MaxLength is int max)
                a.Add(v.MinLength is int min ? $"[StringLength({max}, MinimumLength = {min})]" : $"[StringLength({max})]");
            else if (v.MinLength is int min)
                a.Add($"[MinLength({min})]");
        }
        if ((f.Type is "number") && v is not null && (v.Min is not null || v.Max is not null))
        {
            var lo = v.Min?.ToString(CultureInfo.InvariantCulture) ?? "int.MinValue";
            var hi = v.Max?.ToString(CultureInfo.InvariantCulture) ?? "int.MaxValue";
            a.Add($"[Range({lo}, {hi})]");
        }
        if ((f.Type is "decimal" or "currency") && v is not null && (v.Min is not null || v.Max is not null))
        {
            var lo = (v.Min ?? 0).ToString(CultureInfo.InvariantCulture);
            // decimal.MaxValue exactly (a double literal would round up and overflow).
            var hi = v.Max?.ToString(CultureInfo.InvariantCulture) ?? "79228162514264337593543950335";
            a.Add($"[Range(typeof(decimal), \"{lo}\", \"{hi}\")]");
        }
        if (!string.IsNullOrEmpty(v?.Pattern))
            a.Add($"[RegularExpression(@\"{v!.Pattern.Replace("\"", "\"\"")}\")]");
        return a.Count == 0 ? string.Empty : "    " + string.Join("\n    ", a) + "\n";
    }

    private static string DtoProp(FieldInfo f, bool withAnnotations)
    {
        var line = Prop(f).TrimStart();
        return (withAnnotations ? Annotations(f) : string.Empty) + "    " + line;
    }

    private static string BuildDtos(Manifest m, string ns, List<FieldInfo> fields)
    {
        var dtoProps = string.Join("\n", fields.Select(Prop));
        var createProps = string.Join("\n\n", fields.Select(f => DtoProp(f, true)));
        var updateProps = string.Join("\n\n", fields.Select(f => DtoProp(f, true)));
        return $@"// <auto-generated>Generated by erpgen. DO NOT EDIT.</auto-generated>
#nullable enable
using System.ComponentModel.DataAnnotations;
using ErpBackend.CrossCutting.Common;

namespace {ns};

/// <summary>Read DTO.</summary>
public class {m.Module.Name}Dto : BaseDto
{{
{dtoProps}
}}

/// <summary>Create payload (DataAnnotations route through ValidationErrorResponse via AddErpApiValidation).</summary>
public class Create{m.Module.Name}Dto : BaseCreateDto
{{
{createProps}
}}

/// <summary>Update payload.</summary>
public class Update{m.Module.Name}Dto : BaseUpdateDto
{{
{updateProps}

    public bool IsActive {{ get; set; }} = true;
}}

/// <summary>Filter (search + isActive + created range inherited from FilterParams).</summary>
public class {m.Module.Name}Filter : ErpBackend.CrossCutting.Pagination.FilterParams;
";
    }

    private static string BuildPermissions(Manifest m, string ns)
    {
        var prefix = m.Permissions?.Prefix ?? m.Module.Key;
        return $@"// <auto-generated>Generated by erpgen. DO NOT EDIT.</auto-generated>
#nullable enable
namespace {ns};

public static class {m.Module.Name}Permissions
{{
    public const string View = ""{prefix}.view"";
    public const string Create = ""{prefix}.create"";
    public const string Update = ""{prefix}.update"";
    public const string Delete = ""{prefix}.delete"";
}}
";
    }

    private static string BuildRepositoryInterface(string name, string ns)
        => $@"// <auto-generated>Generated by erpgen. DO NOT EDIT.</auto-generated>
#nullable enable
using ErpBackend.Persistence.Abstractions;

namespace {ns};

/// <summary>Async persistence contract for {name} (Query/GetAsync/AddAsync/UpdateAsync/RemoveAsync
/// inherited from <see cref=""IRepository{{T}}""/>). Implemented by the EF or in-memory repository.</summary>
public interface I{name}Repository : IRepository<{name}>;
";

    private static string ResourceRoute(Manifest m)
    {
        var resource = m.Api?.Resource ?? m.Module.Key;
        return m.Api?.BasePath is { Length: > 0 } bp ? $"{bp}/{resource}" : resource;
    }

    private static string BuildController(Manifest m, string ns, List<FieldInfo> fields, Store store)
    {
        var name = m.Module.Name;
        var route = ResourceRoute(m);
        var stringFields = fields.Where(f => CsType(f) == "string").Select(f => Pascal(f.Name)).ToList();

        // Search predicate (D6): ToLower().Contains is translatable by EF Core AND works on the
        // in-memory IQueryable, so a single generated controller supports both stores. It is not the
        // most optimized form (no index/collation use). FUTURE: a provider-optimized strategy using
        // EF.Functions.Like or a pluggable search abstraction — tracked in docs/PERSISTENCE.md.
        var search = stringFields.Count == 0
            ? string.Empty
            : $@"
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {{
            var term = filter.Search.Trim().ToLower();
            query = query.Where(e => {string.Join(" || ", stringFields.Select(p => $"(e.{p} ?? string.Empty).ToLower().Contains(term)"))});
        }}";

        // EF executes count + page asynchronously; the in-memory store materializes synchronously
        // (ToPagedResultAsync only supports EF queryables).
        var pagingExtraUsing = store == Store.Ef ? "using ErpBackend.Persistence.Extensions;\n" : string.Empty;
        var pagedLine = store == Store.Ef
            ? "var paged = await query.ToPagedResultAsync(filter);"
            : "var paged = query.ApplyPagination(filter);";

        var assignFromCreate = string.Join("\n            ", fields.Select(f => $"{Pascal(f.Name)} = dto.{Pascal(f.Name)},"));
        var assignUpdate = string.Join("\n        ", fields.Select(f => $"entity.{Pascal(f.Name)} = dto.{Pascal(f.Name)};"));
        var toDtoAssign = string.Join("\n        ", fields.Select(f => $"{Pascal(f.Name)} = e.{Pascal(f.Name)},"));

        return $@"// <auto-generated>Generated by erpgen. DO NOT EDIT (custom logic belongs elsewhere).</auto-generated>
#nullable enable
using ErpBackend.CrossCutting.Exceptions;
using ErpBackend.CrossCutting.Extensions;
using ErpBackend.CrossCutting.Pagination;
using ErpBackend.CrossCutting.Responses;
using ErpBackend.CrossCutting.Security;
{pagingExtraUsing}using Microsoft.AspNetCore.Mvc;

namespace {ns};

[ApiController]
[Route(""api/{route}"")]
[Produces(""application/json"")]
public class {name}Controller(I{name}Repository repository) : ControllerBase
{{
    [HttpGet]
    [HasPermission({name}Permissions.View)]
    public async Task<ActionResult<PagedResponse<{name}Dto>>> GetAll([FromQuery] {name}Filter filter)
    {{
        var query = repository.Query();{search}
        if (filter.IsActive.HasValue)
        {{
            query = query.Where(e => e.IsActive == filter.IsActive.Value);
        }}

        {pagedLine}
        var dtos = new PagedResult<{name}Dto>(
            paged.Items.Select(ToDto).ToList(), paged.Meta.Page, paged.Meta.PageSize, paged.Meta.TotalItems);
        return Ok(PagedResponse<{name}Dto>.From(dtos));
    }}

    [HttpGet(""{{id:guid}}"")]
    [HasPermission({name}Permissions.View)]
    public async Task<ActionResult<ApiResponse<{name}Dto>>> GetById(Guid id)
    {{
        var entity = await repository.GetAsync(id) ?? throw new NotFoundException(""{name}"", id);
        return Ok(ApiResponse<{name}Dto>.Ok(ToDto(entity)));
    }}

    [HttpPost]
    [HasPermission({name}Permissions.Create)]
    public async Task<ActionResult<CreatedResponse<Guid>>> Create([FromBody] Create{name}Dto dto)
    {{
        var entity = new {name}
        {{
            {assignFromCreate}
            IsActive = true,
        }};
        await repository.AddAsync(entity);
        return StatusCode(StatusCodes.Status201Created, new CreatedResponse<Guid>(entity.Id));
    }}

    [HttpPut(""{{id:guid}}"")]
    [HasPermission({name}Permissions.Update)]
    public async Task<ActionResult<NoContentResponse>> Update(Guid id, [FromBody] Update{name}Dto dto)
    {{
        var entity = await repository.GetAsync(id) ?? throw new NotFoundException(""{name}"", id);
        {assignUpdate}
        entity.IsActive = dto.IsActive;
        await repository.UpdateAsync(entity);
        return Ok(new NoContentResponse());
    }}

    [HttpDelete(""{{id:guid}}"")]
    [HasPermission({name}Permissions.Delete)]
    public async Task<ActionResult<NoContentResponse>> Delete(Guid id)
    {{
        if (!await repository.RemoveAsync(id))
        {{
            throw new NotFoundException(""{name}"", id);
        }}
        return Ok(new NoContentResponse());
    }}

    private static {name}Dto ToDto({name} e) => new()
    {{
        Id = e.Id,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        IsActive = e.IsActive,
        {toDtoAssign}
    }};
}}
";
    }

    private static string BuildModuleExtensions(string name, string ns, Store store)
    {
        var impl = store == Store.Ef ? $"{name}EfRepository" : $"{name}InMemoryRepository";
        var note = store == Store.Ef
            ? "Uses the EF Core repository (backed by the application DbContext)."
            : "Uses the in-memory repository (prototyping/tests). Generate with the default store for EF Core.";
        return $@"// <auto-generated>Generated by erpgen. DO NOT EDIT.</auto-generated>
#nullable enable
using {ns};

namespace Microsoft.Extensions.DependencyInjection;

public static class {name}ModuleExtensions
{{
    /// <summary>Registers the {name} module's services. {note}</summary>
    public static IServiceCollection Add{name}Module(this IServiceCollection services)
    {{
        services.AddScoped<I{name}Repository, {impl}>();
        return services;
    }}
}}
";
    }

    private static string BuildInMemoryRepository(string name, string ns)
        => $@"// Editable, created once by erpgen (NOT regenerated). Generated only with --store inmemory.
// For production use the default EF Core store (regenerate without --store inmemory).
using System.Collections.Concurrent;

namespace {ns};

public class {name}InMemoryRepository : I{name}Repository
{{
    // Static so data survives across requests regardless of DI lifetime (demo/prototype only).
    private static readonly ConcurrentDictionary<Guid, {name}> _items = new();

    public IQueryable<{name}> Query() => _items.Values.AsQueryable();

    public Task<{name}?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.TryGetValue(id, out var e) ? e : null);

    public Task<{name}> AddAsync({name} entity, CancellationToken cancellationToken = default)
    {{
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.CreatedAt = DateTime.UtcNow;
        _items[entity.Id] = entity;
        return Task.FromResult(entity);
    }}

    public Task<{name}> UpdateAsync({name} entity, CancellationToken cancellationToken = default)
    {{
        entity.UpdatedAt = DateTime.UtcNow;
        _items[entity.Id] = entity;
        return Task.FromResult(entity);
    }}

    public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_items.TryRemove(id, out _));
}}
";

    private static string BuildEfRepository(string name, string ns)
        => $@"// <auto-generated>Generated by erpgen. DO NOT EDIT.</auto-generated>
#nullable enable
using ErpBackend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace {ns};

/// <summary>EF Core repository for {name}, backed by the application's DbContext.</summary>
public class {name}EfRepository(DbContext context) : EfRepository<{name}>(context), I{name}Repository;
";

    private static string BuildEntityConfiguration(Manifest m, string ns, List<FieldInfo> fields)
    {
        var name = m.Module.Name;
        var lines = new List<string>();

        // Explicit max lengths (from validation) keep relational columns sized; decimals use the
        // ErpDbContext global precision convention; primitive collections (multiselect) map to JSON
        // automatically in EF Core. Indexes on searchable string fields keep list filtering reasonable.
        foreach (var f in fields)
        {
            if (CsType(f) == "string" && f.Validation?.MaxLength is int max)
            {
                lines.Add($"builder.Property(e => e.{Pascal(f.Name)}).HasMaxLength({max});");
            }
        }
        foreach (var f in fields.Where(f => CsType(f) == "string"))
        {
            lines.Add($"builder.HasIndex(e => e.{Pascal(f.Name)});");
        }

        var body = lines.Count == 0
            ? "        // No extra mapping required (conventions cover keys, audit, soft-delete, decimals)."
            : "        " + string.Join("\n        ", lines);

        return $@"// <auto-generated>Generated by erpgen. DO NOT EDIT.</auto-generated>
#nullable enable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {ns};

/// <summary>EF Core mapping for {name}. Discovered automatically by ApplyConfigurationsFromAssembly.</summary>
public class {name}Configuration : IEntityTypeConfiguration<{name}>
{{
    public void Configure(EntityTypeBuilder<{name}> builder)
    {{
        builder.ToTable(""{name}"");
{body}
    }}
}}
";
    }

    // ---- file + patch helpers ----------------------------------------------

    // Emit LF regardless of this tool's own source encoding. The Build* methods use verbatim string
    // literals, so their line endings would otherwise inherit Generator.cs's EOL (CRLF on Windows).
    // Forcing LF keeps regeneration byte-stable — no phantom CRLF/LF diff (paired with .gitattributes).
    private static string NormalizeEol(string content) => content.Replace("\r\n", "\n");

    // Writes (or, in dry-run, only classifies) one file. `regen` files are always (re)written;
    // non-regen files (the editable in-memory repo) are kept if they already exist.
    private static FileOp Emit(string dir, string file, string content, bool regen, bool dryRun)
    {
        var path = Path.Combine(dir, file);
        var exists = File.Exists(path);
        if (!regen && exists)
        {
            return new FileOp(file, FileStatus.Kept);
        }
        if (!dryRun)
        {
            File.WriteAllText(path, NormalizeEol(content));
        }
        return new FileOp(file, exists ? FileStatus.Overwritten : FileStatus.Created);
    }

    /// <summary>The anchor line in the target Program.cs where module registrations are inserted.</summary>
    public const string ProgramAnchor = "// erp-generated:modules";

    // Registers the module in Program.cs at the anchor (idempotent). Returns the outcome so the
    // caller can warn loudly when the anchor is missing (the module would compile but never wire up).
    private static ProgramStatus PatchProgram(string projectDir, string name, bool dryRun)
    {
        var programPath = Path.Combine(projectDir, "Program.cs");
        if (!File.Exists(programPath))
        {
            return ProgramStatus.NoProgramFile;
        }
        var content = File.ReadAllText(programPath);
        if (content.Contains($"Add{name}Module("))
        {
            return ProgramStatus.AlreadyRegistered;
        }
        if (!content.Contains(ProgramAnchor))
        {
            return ProgramStatus.AnchorMissing;
        }
        if (dryRun)
        {
            return ProgramStatus.Patched;
        }
        var line = $"builder.Services.Add{name}Module();\n";
        var idx = content.IndexOf(ProgramAnchor, StringComparison.Ordinal);
        var lineStart = content.LastIndexOf('\n', idx) + 1;
        content = content.Insert(lineStart, line);
        File.WriteAllText(programPath, content);
        return ProgramStatus.Patched;
    }
}
