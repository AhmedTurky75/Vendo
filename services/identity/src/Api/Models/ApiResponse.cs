namespace Vendo.Identity.Api.Models;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Validation errors if any
    /// </summary>
    public List<string>? ValidationErrors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string error, List<string>? validationErrors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error,
            ValidationErrors = validationErrors
        };
    }
}
