namespace Vendo.CatalogManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing product dimensions (Length x Width x Height).
/// All measurements are in centimeters.
/// </summary>
public sealed class Dimensions : IEquatable<Dimensions>
{
    public decimal Length { get; }
    public decimal Width { get; }
    public decimal Height { get; }
    public string Unit { get; }

    private Dimensions(decimal length, decimal width, decimal height, string unit = "cm")
    {
        Length = length;
        Width = width;
        Height = height;
        Unit = unit;
    }

    /// <summary>
    /// Creates a new Dimensions instance.
    /// </summary>
    public static Dimensions? Create(decimal length, decimal width, decimal height, string unit = "cm")
    {
        if (length <= 0 || width <= 0 || height <= 0)
            return null;

        if (string.IsNullOrWhiteSpace(unit))
            return null;

        return new Dimensions(length, width, height, unit.ToLowerInvariant());
    }

    /// <summary>
    /// Parses dimensions from string format "LxWxH" or "L x W x H cm".
    /// </summary>
    public static Dimensions? Parse(string dimensionsString)
    {
        if (string.IsNullOrWhiteSpace(dimensionsString))
            return null;

        // Remove unit if present
        var unit = "cm";
        var cleanString = dimensionsString.Trim().ToLowerInvariant();

        if (cleanString.EndsWith("cm"))
        {
            cleanString = cleanString[..^2].Trim();
            unit = "cm";
        }
        else if (cleanString.EndsWith("in") || cleanString.EndsWith("inch"))
        {
            cleanString = cleanString.Replace("inch", "").Replace("in", "").Trim();
            unit = "in";
        }

        // Split by 'x' or 'X'
        var parts = cleanString.Split(new[] { 'x', 'X', '×' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
            return null;

        if (!decimal.TryParse(parts[0].Trim(), out var length))
            return null;

        if (!decimal.TryParse(parts[1].Trim(), out var width))
            return null;

        if (!decimal.TryParse(parts[2].Trim(), out var height))
            return null;

        return Create(length, width, height, unit);
    }

    /// <summary>
    /// Calculates the volume (L x W x H).
    /// </summary>
    public decimal CalculateVolume()
    {
        return Length * Width * Height;
    }

    /// <summary>
    /// Gets the dimensions in string format.
    /// </summary>
    public override string ToString()
    {
        return $"{Length} x {Width} x {Height} {Unit}";
    }

    /// <summary>
    /// Gets a compact string representation.
    /// </summary>
    public string ToCompactString()
    {
        return $"{Length}x{Width}x{Height}{Unit}";
    }

    public bool Equals(Dimensions? other)
    {
        if (other is null)
            return false;

        return Length == other.Length
               && Width == other.Width
               && Height == other.Height
               && Unit == other.Unit;
    }

    public override bool Equals(object? obj)
    {
        return obj is Dimensions dimensions && Equals(dimensions);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Length, Width, Height, Unit);
    }

    public static bool operator ==(Dimensions? left, Dimensions? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Dimensions? left, Dimensions? right)
    {
        return !(left == right);
    }
}
