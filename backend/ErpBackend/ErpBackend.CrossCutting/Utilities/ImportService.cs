using System.Text;
using ErpBackend.CrossCutting.Helpers;

namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Default CSV import service with a small RFC-4180-aware parser (handles quoted fields,
/// embedded delimiters/newlines and escaped quotes). Header matching is case-insensitive.
/// </summary>
public class ImportService : IImportService
{
    public ImportResult<T> ImportCsv<T>(
        Stream csvStream,
        Func<IReadOnlyDictionary<string, string>, T> map,
        IReadOnlyCollection<string>? requiredHeaders = null,
        char delimiter = ',')
    {
        var result = new ImportResult<T>();

        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = reader.ReadToEnd();

        var records = CsvHelper.Parse(content, delimiter);
        if (records.Count == 0)
        {
            result.Errors.Add(new ImportRowError(0, "The file is empty."));
            return result;
        }

        var headers = records[0].Select(h => h.Trim()).ToList();

        if (requiredHeaders is not null)
        {
            var missing = requiredHeaders
                .Where(req => !headers.Contains(req, StringComparer.OrdinalIgnoreCase))
                .ToList();
            if (missing.Count > 0)
            {
                result.Errors.Add(new ImportRowError(0, $"Missing required headers: {string.Join(", ", missing)}."));
                return result;
            }
        }

        for (var i = 1; i < records.Count; i++)
        {
            var rowNumber = i; // 1-based data row (header excluded)
            var fields = records[i];

            // Skip fully empty lines.
            if (fields.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            result.TotalRows++;

            var rowDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var c = 0; c < headers.Count; c++)
            {
                rowDict[headers[c]] = c < fields.Count ? fields[c] : string.Empty;
            }

            try
            {
                result.Items.Add(map(rowDict));
                result.SuccessfulRows++;
            }
            catch (Exception ex)
            {
                result.FailedRows++;
                result.Errors.Add(new ImportRowError(rowNumber, ex.Message));
            }
        }

        return result;
    }
}
