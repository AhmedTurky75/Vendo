using System.Text.RegularExpressions;

namespace Vendo.CatalogManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing a Stock Keeping Unit (SKU).
/// Ensures SKU format is valid and consistent.
/// </summary>
public sealed class SKU : IEquatable<SKU>
{
    private static readonly Regex SkuPattern = new(@"^[A-Z0-9\-_]{3,50}$", RegexOptions.Compiled);

    public string Value { get; }

    private SKU(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new SKU instance.
    /// </summary>
    /// <param name="value">The SKU value</param>
    /// <returns>SKU instance if valid, otherwise null</returns>
    public static SKU? Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Normalize to uppercase
        var normalized = value.Trim().ToUpperInvariant();

        // Validate format
        if (!SkuPattern.IsMatch(normalized))
            return null;

        return new SKU(normalized);
    }

    /// <summary>
    /// Generates a new random SKU.
    /// </summary>
    public static SKU Generate(string? prefix = null)
    {
        var timestamp = DateTime.UtcNow.Ticks.ToString("X");
        var random = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var sku = string.IsNullOrWhiteSpace(prefix)
            ? $"SKU-{timestamp}-{random}"
            : $"{prefix.ToUpperInvariant()}-{timestamp}-{random}";

        return new SKU(sku);
    }

    public bool Equals(SKU? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is SKU sku && Equals(sku);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(SKU? left, SKU? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(SKU? left, SKU? right)
    {
        return !(left == right);
    }

    public static implicit operator string(SKU sku) => sku.Value;
}
