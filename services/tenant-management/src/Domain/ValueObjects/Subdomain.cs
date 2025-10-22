using System.Text.RegularExpressions;

namespace Vendo.TenantManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing a store's subdomain.
/// Ensures subdomain follows URL-safe naming conventions.
/// </summary>
public sealed class Subdomain : IEquatable<Subdomain>
{
    private static readonly Regex SubdomainRegex = new(@"^[a-z0-9](?:[a-z0-9-]{1,61}[a-z0-9])?$", RegexOptions.Compiled);

    private static readonly HashSet<string> ReservedSubdomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "api", "www", "app", "mail", "ftp", "localhost",
        "vendo", "test", "dev", "stage", "staging", "prod", "production",
        "dashboard", "billing", "support", "help", "docs", "blog"
    };

    /// <summary>
    /// Gets the subdomain value.
    /// </summary>
    public string Value { get; }

    private Subdomain(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Subdomain instance from the given value.
    /// </summary>
    /// <param name="value">The subdomain value.</param>
    /// <returns>A result containing the Subdomain if valid, or an error message if invalid.</returns>
    public static Result<Subdomain> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Subdomain>.Failure("Subdomain cannot be empty");
        }

        value = value.ToLowerInvariant().Trim();

        if (value.Length < 3)
        {
            return Result<Subdomain>.Failure("Subdomain must be at least 3 characters long");
        }

        if (value.Length > 63)
        {
            return Result<Subdomain>.Failure("Subdomain cannot exceed 63 characters");
        }

        if (!SubdomainRegex.IsMatch(value))
        {
            return Result<Subdomain>.Failure("Subdomain can only contain lowercase letters, numbers, and hyphens. It must start and end with a letter or number.");
        }

        if (ReservedSubdomains.Contains(value))
        {
            return Result<Subdomain>.Failure($"Subdomain '{value}' is reserved and cannot be used");
        }

        return Result<Subdomain>.Success(new Subdomain(value));
    }

    public bool Equals(Subdomain? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is Subdomain other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Value;

    public static implicit operator string(Subdomain subdomain) => subdomain.Value;
}

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
