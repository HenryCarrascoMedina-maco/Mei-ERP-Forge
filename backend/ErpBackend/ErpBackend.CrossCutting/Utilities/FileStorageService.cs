using ErpBackend.CrossCutting.Exceptions;
using ErpBackend.CrossCutting.Helpers;
using Microsoft.Extensions.Options;

namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Local-disk implementation of <see cref="IFileStorageService"/>. Stores files under a
/// configurable base path, generating safe file names and guarding against path traversal.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public FileStorageService(IOptions<FileStorageOptions> options)
    {
        var basePath = options.Value.BasePath;
        _rootPath = Path.IsPathRooted(basePath)
            ? basePath
            : Path.Combine(AppContext.BaseDirectory, basePath);
    }

    public async Task<StoredFileInfo> SaveAsync(
        Stream content,
        string originalFileName,
        string? subDirectory = null,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var safeName = FileHelper.GenerateSafeFileName(originalFileName);
        var relativePath = string.IsNullOrWhiteSpace(subDirectory)
            ? safeName
            : $"{NormalizeSegment(subDirectory)}/{safeName}";

        var fullPath = ResolveFullPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var info = new FileInfo(fullPath);
        return new StoredFileInfo
        {
            FileName = safeName,
            RelativePath = relativePath,
            SizeBytes = info.Length,
            ContentType = contentType,
            CreatedAtUtc = info.CreationTimeUtc,
        };
    }

    public async Task<byte[]> ReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            throw new NotFoundException("File", relativePath);
        }
        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    public bool Exists(string relativePath)
        => File.Exists(ResolveFullPath(relativePath));

    public StoredFileInfo? GetMetadata(string relativePath)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            return null;
        }
        var info = new FileInfo(fullPath);
        return new StoredFileInfo
        {
            FileName = info.Name,
            RelativePath = relativePath,
            SizeBytes = info.Length,
            CreatedAtUtc = info.CreationTimeUtc,
        };
    }

    /// <summary>Resolves and validates a relative path stays inside the storage root.</summary>
    private string ResolveFullPath(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        var combined = Path.GetFullPath(Path.Combine(_rootPath, normalized));
        var rootFull = Path.GetFullPath(_rootPath);

        if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Invalid file path.", "INVALID_PATH");
        }
        return combined;
    }

    private static string NormalizeSegment(string segment)
        => segment.Replace('\\', '/').Trim('/');
}
