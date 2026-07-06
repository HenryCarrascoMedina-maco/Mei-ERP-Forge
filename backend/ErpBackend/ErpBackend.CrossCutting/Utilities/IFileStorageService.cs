namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Abstraction over file persistence. The default implementation stores files on the local disk;
/// swap it for a cloud-backed implementation without touching callers.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Saves a stream and returns its metadata (including the relative handle).</summary>
    Task<StoredFileInfo> SaveAsync(
        Stream content,
        string originalFileName,
        string? subDirectory = null,
        string? contentType = null,
        CancellationToken cancellationToken = default);

    /// <summary>Reads the file's bytes by its relative handle.</summary>
    Task<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Deletes the file by its relative handle. No-op when it does not exist.</summary>
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>Whether a file exists for the given relative handle.</summary>
    bool Exists(string relativePath);

    /// <summary>Returns metadata for a stored file, or null when it does not exist.</summary>
    StoredFileInfo? GetMetadata(string relativePath);
}
