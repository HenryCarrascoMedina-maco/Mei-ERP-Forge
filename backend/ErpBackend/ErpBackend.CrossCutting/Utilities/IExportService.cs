namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Generic export service. CSV is supported out of the box; Excel and PDF are extension points
/// that throw <see cref="NotSupportedException"/> until a concrete implementation (and its
/// package) is provided.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports a collection to CSV. When <paramref name="columns"/> is null, all public readable
    /// properties of <typeparamref name="T"/> are exported.
    /// </summary>
    ExportResult ToCsv<T>(
        IEnumerable<T> items,
        IReadOnlyList<ExportColumn<T>>? columns = null,
        string fileName = "export.csv");

    /// <summary>Exports to Excel (.xlsx). Requires a spreadsheet package implementation.</summary>
    ExportResult ToExcel<T>(
        IEnumerable<T> items,
        IReadOnlyList<ExportColumn<T>>? columns = null,
        string fileName = "export.xlsx");

    /// <summary>Exports to PDF. Requires a PDF package implementation.</summary>
    ExportResult ToPdf<T>(
        IEnumerable<T> items,
        IReadOnlyList<ExportColumn<T>>? columns = null,
        string fileName = "export.pdf");
}
