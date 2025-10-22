namespace Vendo.TenantManagement.Application.Common.Models;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class Result
{
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    protected Result(bool isSuccess, string? error, IReadOnlyList<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? Array.Empty<string>();
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    public static Result Failure(IReadOnlyList<string> errors) =>
        new(false, string.Join("; ", errors), errors);
}

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Gets the value if the operation succeeded.
    /// </summary>
    public T? Value { get; }

    protected Result(bool isSuccess, T? value, string? error, IReadOnlyList<string>? errors = null)
        : base(isSuccess, error, errors)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public new static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Creates a failed result with multiple error messages.
    /// </summary>
    public new static Result<T> Failure(IReadOnlyList<string> errors) =>
        new(false, default, string.Join("; ", errors), errors);
}
