using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ErpBackend.CrossCutting.Helpers;

/// <summary>
/// Text cleaning, normalization and slug generation helpers.
/// </summary>
public static partial class StringHelper
{
    public static bool IsBlank(string? value) => string.IsNullOrWhiteSpace(value);

    public static string Capitalize(string value)
        => string.IsNullOrEmpty(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];

    /// <summary>Collapses runs of whitespace into single spaces and trims.</summary>
    public static string NormalizeWhitespace(string value)
        => WhitespaceRegex().Replace(value ?? string.Empty, " ").Trim();

    /// <summary>Removes diacritics (accents) using Unicode normalization.</summary>
    public static string RemoveAccents(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>URL-friendly slug: lowercase, accent-free, hyphen-separated.</summary>
    public static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }
        var slug = RemoveAccents(value).ToLowerInvariant();
        slug = NonAlphanumericRegex().Replace(slug, "-");
        return slug.Trim('-');
    }

    /// <summary>Truncates with an ellipsis when longer than <paramref name="maxLength"/>.</summary>
    public static string Truncate(string value, int maxLength, string ellipsis = "…")
        => string.IsNullOrEmpty(value) || value.Length <= maxLength
            ? value
            : value[..maxLength].TrimEnd() + ellipsis;

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();
}
