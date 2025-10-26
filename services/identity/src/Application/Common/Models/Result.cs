namespace Vendo.IdentityManagement.Application.Common.Models;

/// <summary>
/// Generic result wrapper for application operations
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public List<string> ValidationErrors { get; }

    private Result(bool isSuccess, T? data, string? error, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> ValidationFailure(List<string> validationErrors) =>
        new(false, default, "Validation failed", validationErrors);
}

/// <summary>
/// Non-generic result wrapper for operations without return value
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public List<string> ValidationErrors { get; }

    private Result(bool isSuccess, string? error, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result ValidationFailure(List<string> validationErrors) =>
        new(false, "Validation failed", validationErrors);
}
