using System.Reflection;
using System.Text;
using ErpBackend.CrossCutting.Constants;
using ErpBackend.CrossCutting.Helpers;

namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Default export service. Implements CSV generation with reflection-based column discovery and
/// RFC-4180 escaping. Excel/PDF throw <see cref="NotSupportedException"/> — override this service
/// (or register a richer implementation) once the corresponding package is added.
/// </summary>
public class ExportService : IExportService
{
    public ExportResult ToCsv<T>(
        IEnumerable<T> items,
        IReadOnlyList<ExportColumn<T>>? columns = null,
        string fileName = "export.csv")
    {
        var resolved = columns ?? BuildColumnsFromProperties<T>();

        var builder = new StringBuilder();
        builder.AppendLine(CsvHelper.BuildLine(resolved.Select(c => (object?)c.Header)));

        foreach (var item in items)
        {
            builder.AppendLine(CsvHelper.BuildLine(resolved.Select(c => c.Value(item))));
        }

        // Prepend a UTF-8 BOM so Excel opens accented characters correctly.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();

        return new ExportResult
        {
            Content = bytes,
            FileName = fileName,
            ContentType = HttpConstants.ContentTypes.Csv,
        };
    }

    public virtual ExportResult ToExcel<T>(
        IEnumerable<T> items,
        IReadOnlyList<ExportColumn<T>>? columns = null,
        string fileName = "export.xlsx")
        => throw new NotSupportedException(
            "Excel export is not implemented. Add a spreadsheet package (e.g. ClosedXML/EPPlus) " +
            "and override ExportService.ToExcel.");

    public virtual ExportResult ToPdf<T>(
        IEnumerable<T> items,
        IReadOnlyList<ExportColumn<T>>? columns = null,
        string fileName = "export.pdf")
        => throw new NotSupportedException(
            "PDF export is not implemented. Add a PDF package (e.g. QuestPDF) and override ExportService.ToPdf.");

    private static IReadOnlyList<ExportColumn<T>> BuildColumnsFromProperties<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Select(p => new ExportColumn<T>(p.Name, item => p.GetValue(item)))
            .ToList();
    }
}
