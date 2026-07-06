namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Generic CSV import service. Parses a CSV stream, validates required headers and maps each row
/// to the target type via a caller-supplied delegate, collecting per-row errors.
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Imports rows from a CSV stream.
    /// </summary>
    /// <param name="csvStream">The CSV content.</param>
    /// <param name="map">
    /// Maps a row (header → value, case-insensitive) to <typeparamref name="T"/>. Throw to fail
    /// the row; the thrown message is recorded against that row.
    /// </param>
    /// <param name="requiredHeaders">Headers that must be present; missing ones fail the whole import.</param>
    /// <param name="delimiter">Field delimiter (default comma).</param>
    ImportResult<T> ImportCsv<T>(
        Stream csvStream,
        Func<IReadOnlyDictionary<string, string>, T> map,
        IReadOnlyCollection<string>? requiredHeaders = null,
        char delimiter = ',');
}
