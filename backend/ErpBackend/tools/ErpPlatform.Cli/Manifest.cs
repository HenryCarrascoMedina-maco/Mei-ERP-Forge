namespace ErpPlatform.Cli;

/// <summary>POCO mirror of the module manifest (the JSON Schema is the source of truth).</summary>
public sealed class Manifest
{
    public string SchemaVersion { get; set; } = "1.0";
    public ModuleInfo Module { get; set; } = new();
    public EntityInfo Entity { get; set; } = new();
    public PermissionsInfo? Permissions { get; set; }
    public ApiInfo? Api { get; set; }
    public FeaturesInfo? Features { get; set; }
}

public sealed class ModuleInfo
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? PluralName { get; set; }
}

public sealed class EntityInfo
{
    public string? IdType { get; set; }
    public string? Base { get; set; }
    public List<FieldInfo> Fields { get; set; } = new();
}

public sealed class FieldInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public string Label { get; set; } = string.Empty;
    public bool Required { get; set; }
    public bool Unique { get; set; }
    public ValidationInfo? Validation { get; set; }
    public List<OptionInfo>? Options { get; set; }
}

public sealed class ValidationInfo
{
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public string? Pattern { get; set; }
    public bool? Email { get; set; }
}

public sealed class OptionInfo
{
    public object? Value { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class PermissionsInfo
{
    public string? Prefix { get; set; }
}

public sealed class ApiInfo
{
    public string? BasePath { get; set; }
    public string? Resource { get; set; }
}

public sealed class FeaturesInfo
{
    public bool? Exportable { get; set; }
    public bool? Importable { get; set; }
}
