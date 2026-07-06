namespace ErpBackend.CrossCutting.Utilities;

/// <summary>
/// Result of an export operation: the file bytes plus the suggested name and content type.
/// </summary>
public class ExportResult
{
    public byte[] Content { get; set; } = Array.Empty<byte>();

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";
}

/// <summary>
/// Defines a single export column: its header and how to extract the cell value from a row.
/// </summary>
public class ExportColumn<T>
{
    public string Header { get; }

    public Func<T, object?> Value { get; }

    public ExportColumn(string header, Func<T, object?> value)
    {
        Header = header;
        Value = value;
    }
}
