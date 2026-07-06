namespace ErpBackend.CrossCutting.Utilities;

/// <summary>An error tied to a specific imported row (and optionally a column).</summary>
public class ImportRowError
{
    /// <summary>1-based data row number (excludes the header).</summary>
    public int Row { get; set; }

    public string? Column { get; set; }

    public string Message { get; set; } = string.Empty;

    public ImportRowError() { }

    public ImportRowError(int row, string message, string? column = null)
    {
        Row = row;
        Message = message;
        Column = column;
    }
}

/// <summary>
/// Outcome of an import: counts, the successfully mapped items and per-row errors.
/// </summary>
public class ImportResult<T>
{
    public int TotalRows { get; set; }

    public int SuccessfulRows { get; set; }

    public int FailedRows { get; set; }

    public List<T> Items { get; set; } = new();

    public List<ImportRowError> Errors { get; set; } = new();

    public bool HasErrors => Errors.Count > 0;
}
