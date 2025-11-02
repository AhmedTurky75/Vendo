using System.Text;
using System.Text.RegularExpressions;

namespace Vendo.CatalogManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing a URL-friendly slug.
/// Ensures slugs are properly formatted for SEO.
/// </summary>
public sealed class Slug : IEquatable<Slug>
{
    private static readonly Regex SlugPattern = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Slug instance from a string.
    /// </summary>
    /// <param name="value">The slug value</param>
    /// <returns>Slug instance if valid, otherwise null</returns>
    public static Slug? Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();

        if (!SlugPattern.IsMatch(normalized))
            return null;

        return new Slug(normalized);
    }

    /// <summary>
    /// Generates a slug from any text.
    /// </summary>
    /// <param name="text">The text to convert to a slug</param>
    /// <returns>A valid Slug instance</returns>
    public static Slug? Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        // Convert to lowercase
        var slug = text.ToLowerInvariant();

        // Remove accents
        slug = RemoveAccents(slug);

        // Replace invalid characters with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

        // Replace multiple spaces or hyphens with single hyphen
        slug = Regex.Replace(slug, @"[\s-]+", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 100)
            slug = slug[..100].TrimEnd('-');

        if (string.IsNullOrEmpty(slug))
            return null;

        return new Slug(slug);
    }

    private static string RemoveAccents(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public bool Equals(Slug? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Slug slug && Equals(slug);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static bool operator ==(Slug? left, Slug? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Slug? left, Slug? right)
    {
        return !(left == right);
    }

    public static implicit operator string(Slug slug) => slug.Value;
}
