namespace Vendo.Catalog.Application.Common;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the result value.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string? Error { get; private set; }

    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public List<string>? Errors { get; private set; }

    private Result(bool isSuccess, T? value, string? error, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    public static Result<T> Failure(string error, List<string> errors) => new(false, default, error, errors);
}
