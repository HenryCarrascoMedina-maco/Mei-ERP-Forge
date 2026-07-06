using System.Text.RegularExpressions;

namespace ErpBackend.CrossCutting.Helpers;

/// <summary>
/// Pure helpers for validating and normalizing file names, extensions, sizes and content types.
/// Contains no I/O — see <c>IFileStorageService</c> for persistence.
/// </summary>
public static partial class FileHelper
{
    /// <summary>Returns the lowercase extension without the dot (e.g. "pdf"), or empty when none.</summary>
    public static string GetExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return string.IsNullOrEmpty(ext) ? string.Empty : ext.TrimStart('.').ToLowerInvariant();
    }

    /// <summary>True when the file's extension is in <paramref name="allowedExtensions"/> (case-insensitive).</summary>
    public static bool IsAllowedExtension(string fileName, IEnumerable<string> allowedExtensions)
    {
        var ext = GetExtension(fileName);
        return allowedExtensions
            .Select(e => e.TrimStart('.').ToLowerInvariant())
            .Contains(ext);
    }

    /// <summary>True when the size (bytes) does not exceed <paramref name="maxSizeMb"/>.</summary>
    public static bool IsWithinSize(long sizeBytes, double maxSizeMb)
        => sizeBytes <= (long)(maxSizeMb * 1024 * 1024);

    /// <summary>True when the content type is in <paramref name="allowedContentTypes"/> (case-insensitive).</summary>
    public static bool IsAllowedContentType(string? contentType, IEnumerable<string> allowedContentTypes)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }
        return allowedContentTypes.Any(c => string.Equals(c, contentType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Normalizes a file name: strips the directory, replaces unsafe characters with underscores
    /// and collapses whitespace. Preserves the original extension.
    /// </summary>
    public static string NormalizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        name = UnsafeCharsRegex().Replace(name, "_");
        name = WhitespaceRegex().Replace(name, "_");
        return name.Trim('_', '.', ' ');
    }

    /// <summary>
    /// Generates a collision-resistant, safe storage file name: a normalized stem, a short unique
    /// suffix and the original (lowercased) extension.
    /// </summary>
    public static string GenerateSafeFileName(string originalFileName)
    {
        var ext = GetExtension(originalFileName);
        var stem = Path.GetFileNameWithoutExtension(NormalizeFileName(originalFileName));
        if (string.IsNullOrWhiteSpace(stem))
        {
            stem = "file";
        }
        var unique = Guid.NewGuid().ToString("N")[..12];
        return string.IsNullOrEmpty(ext) ? $"{stem}_{unique}" : $"{stem}_{unique}.{ext}";
    }

    [GeneratedRegex(@"[^a-zA-Z0-9._-]")]
    private static partial Regex UnsafeCharsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
