namespace ErpBackend.CrossCutting.Common;

/// <summary>
/// Represents the outcome of an operation that returns no data.
/// </summary>
public class Result
{
    public bool Succeeded { get; protected set; }

    public string? Message { get; protected set; }

    public IReadOnlyList<string> Errors { get; protected set; } = Array.Empty<string>();

    protected Result(bool succeeded, string? message, IEnumerable<string>? errors = null)
    {
        Succeeded = succeeded;
        Message = message;
        Errors = errors?.ToArray() ?? Array.Empty<string>();
    }

    public static Result Success(string? message = null) => new(true, message);

    public static Result Failure(string message, IEnumerable<string>? errors = null) =>
        new(false, message, errors);
}
