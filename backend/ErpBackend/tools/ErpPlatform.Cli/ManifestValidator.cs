using System.Text.RegularExpressions;

namespace ErpPlatform.Cli;

/// <summary>
/// Validates a parsed <see cref="Manifest"/> before generation, mirroring the rules the JSON Schema
/// (module-manifest.schema.json) already documents. Catches the mistakes that would otherwise produce
/// broken C# or a cryptic failure, and returns actionable, per-field messages. No external dependency.
/// </summary>
public static partial class ManifestValidator
{
    /// <summary>Field types supported by both the frontend schematic and the backend generator.</summary>
    public static readonly string[] SupportedTypes =
    [
        "text", "textarea", "number", "decimal", "currency", "boolean",
        "date", "datetime", "email", "password", "select", "multiselect",
    ];

    /// <summary>Entity base selectors accepted by the generator (null/absent = auditable).</summary>
    private static readonly string[] SupportedBases = ["base", "auditable", "softDelete", "catalog"];

    // Base-class members that would COLLIDE with the generated entity (the generator does not filter
    // these), so declaring them is a real error. Note: id/createdAt/updatedAt/isActive are intentionally
    // NOT here — the generator filters them out silently and the frontend legitimately uses e.g.
    // isActive as a status column, so the shipped example manifests declare it.
    private static readonly HashSet<string> ReservedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "createdBy", "updatedBy", "deletedAt", "deletedBy", "isDeleted",
    };

    /// <summary>Returns a list of human-readable problems; empty means the manifest is valid.</summary>
    public static List<string> Validate(Manifest m)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(m.Module.Name) || !PascalCase().IsMatch(m.Module.Name))
        {
            errors.Add($"module.name '{m.Module.Name}' must be PascalCase (letters/digits, starting uppercase), e.g. \"Customer\".");
        }

        if (string.IsNullOrWhiteSpace(m.Module.Key) || !KebabLower().IsMatch(m.Module.Key))
        {
            errors.Add($"module.key '{m.Module.Key}' must be lowercase kebab-case (letters/digits/hyphen), e.g. \"customers\".");
        }

        if (!string.IsNullOrWhiteSpace(m.Entity.Base) &&
            !SupportedBases.Contains(m.Entity.Base, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"entity.base '{m.Entity.Base}' is not supported. Use one of: {string.Join(", ", SupportedBases)}.");
        }

        if (m.Entity.Fields.Count == 0)
        {
            errors.Add("entity.fields must declare at least one field.");
            return errors;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < m.Entity.Fields.Count; i++)
        {
            var f = m.Entity.Fields[i];
            var at = $"entity.fields[{i}]" + (string.IsNullOrWhiteSpace(f.Name) ? "" : $" '{f.Name}'");

            if (string.IsNullOrWhiteSpace(f.Name) || !CamelCase().IsMatch(f.Name))
            {
                errors.Add($"{at}: name must be camelCase (letters/digits, starting lowercase), e.g. \"unitPrice\".");
            }
            else if (ReservedFields.Contains(f.Name))
            {
                errors.Add($"{at}: '{f.Name}' is provided automatically by the platform — remove it.");
            }
            else if (!seen.Add(f.Name))
            {
                errors.Add($"{at}: duplicate field name '{f.Name}'.");
            }

            if (!SupportedTypes.Contains(f.Type, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"{at}: unsupported type '{f.Type}'. Supported: {string.Join(", ", SupportedTypes)}.");
            }

            if (f.Type is "select" or "multiselect" && (f.Options is null || f.Options.Count == 0))
            {
                errors.Add($"{at}: a '{f.Type}' field must declare at least one option.");
            }
        }

        return errors;
    }

    [GeneratedRegex("^[A-Z][A-Za-z0-9]*$")]
    private static partial Regex PascalCase();

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex KebabLower();

    [GeneratedRegex("^[a-z][A-Za-z0-9]*$")]
    private static partial Regex CamelCase();
}
