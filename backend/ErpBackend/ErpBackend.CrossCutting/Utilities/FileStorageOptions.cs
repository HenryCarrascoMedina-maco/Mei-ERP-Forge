namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Configuration for <see cref="FileStorageService"/>. Bind from configuration
/// (section "FileStorage") or configure in code.
/// </summary>
public class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Root directory where files are stored. Relative paths resolve from the app base directory.</summary>
    public string BasePath { get; set; } = Path.Combine("App_Data", "uploads");
}
