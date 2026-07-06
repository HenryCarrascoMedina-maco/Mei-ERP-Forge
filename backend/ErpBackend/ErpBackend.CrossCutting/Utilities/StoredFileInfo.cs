namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Metadata describing a stored file. <see cref="RelativePath"/> is the handle used to read or
/// delete the file later.
/// </summary>
public class StoredFileInfo
{
    public string FileName { get; set; } = string.Empty;

    /// <summary>Path relative to the storage root (forward-slash separated). Use as the file handle.</summary>
    public string RelativePath { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string? ContentType { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
