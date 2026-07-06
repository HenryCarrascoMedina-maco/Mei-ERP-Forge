namespace ErpBackend.CrossCutting.Common;

/// <summary>
/// Represents the outcome of an operation that returns data of type <typeparamref name="T"/>.
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(bool succeeded, T? data, string? message, IEnumerable<string>? errors = null)
        : base(succeeded, message, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string? message = null) =>
        new(true, data, message);

    public static new Result<T> Failure(string message, IEnumerable<string>? errors = null) =>
        new(false, default, message, errors);
}
