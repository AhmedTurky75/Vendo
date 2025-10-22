using System.Text.RegularExpressions;
using Vendo.Identity.Domain.Exceptions;

namespace Vendo.Identity.Domain.ValueObjects;

/// <summary>
/// Value object representing a password with validation rules
/// </summary>
public sealed class Password : IEquatable<Password>
{
    private const int MinLength = 8;
    private const int MaxLength = 100;

    private static readonly Regex UpperCaseRegex = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex LowerCaseRegex = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);
    private static readonly Regex SpecialCharRegex = new(@"[!@#$%^&*(),.?""':{}|<>]", RegexOptions.Compiled);

    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Password Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new DomainValidationException("Password cannot be empty");
        }

        if (password.Length < MinLength)
        {
            throw new DomainValidationException($"Password must be at least {MinLength} characters long");
        }

        if (password.Length > MaxLength)
        {
            throw new DomainValidationException($"Password cannot exceed {MaxLength} characters");
        }

        if (!UpperCaseRegex.IsMatch(password))
        {
            throw new DomainValidationException("Password must contain at least one uppercase letter");
        }

        if (!LowerCaseRegex.IsMatch(password))
        {
            throw new DomainValidationException("Password must contain at least one lowercase letter");
        }

        if (!DigitRegex.IsMatch(password))
        {
            throw new DomainValidationException("Password must contain at least one digit");
        }

        if (!SpecialCharRegex.IsMatch(password))
        {
            throw new DomainValidationException("Password must contain at least one special character");
        }

        return new Password(password);
    }

    public bool Equals(Password? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Password);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(Password? left, Password? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(Password? left, Password? right)
    {
        return !(left == right);
    }
}
