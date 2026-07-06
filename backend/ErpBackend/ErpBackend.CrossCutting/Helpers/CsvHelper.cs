using System.Globalization;
using System.Text;

namespace ErpBackend.CrossCutting.Helpers;

/// <summary>
/// Low-level CSV generation and parsing (RFC-4180): field escaping, line building and a parser
/// that handles quoted fields, embedded delimiters/newlines and escaped quotes. Shared by the
/// export and import services.
/// </summary>
public static class CsvHelper
{
    /// <summary>Escapes a single value for CSV output.</summary>
    public static string Escape(object? value)
    {
        var text = value switch
        {
            null => string.Empty,
            DateTime dt => dt.ToString("o"),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };

        if (text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r'))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
        return text;
    }

    /// <summary>Builds one CSV line from a set of values.</summary>
    public static string BuildLine(IEnumerable<object?> values, char delimiter = ',')
        => string.Join(delimiter, values.Select(Escape));

    /// <summary>
    /// Parses CSV text into rows of string fields. Handles quotes, escaped quotes ("") and
    /// embedded delimiters/newlines inside quoted fields.
    /// </summary>
    public static List<List<string>> Parse(string content, char delimiter = ',')
    {
        var rows = new List<List<string>>();
        var current = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < content.Length; i++)
        {
            var ch = content[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    field.Append(ch);
                }
                continue;
            }

            switch (ch)
            {
                case '"':
                    inQuotes = true;
                    break;
                case '\r':
                    break;
                case '\n':
                    current.Add(field.ToString());
                    field.Clear();
                    rows.Add(current);
                    current = new List<string>();
                    break;
                default:
                    if (ch == delimiter)
                    {
                        current.Add(field.ToString());
                        field.Clear();
                    }
                    else
                    {
                        field.Append(ch);
                    }
                    break;
            }
        }

        if (field.Length > 0 || current.Count > 0)
        {
            current.Add(field.ToString());
            rows.Add(current);
        }

        return rows;
    }
}
