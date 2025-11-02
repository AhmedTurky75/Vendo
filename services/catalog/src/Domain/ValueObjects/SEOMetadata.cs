namespace Vendo.CatalogManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing SEO metadata for products and categories.
/// </summary>
public sealed class SEOMetadata : IEquatable<SEOMetadata>
{
    public string? MetaTitle { get; }
    public string? MetaDescription { get; }
    public string? MetaKeywords { get; }

    private SEOMetadata(string? metaTitle, string? metaDescription, string? metaKeywords)
    {
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        MetaKeywords = metaKeywords;
    }

    /// <summary>
    /// Creates a new SEO metadata instance.
    /// </summary>
    public static SEOMetadata Create(string? metaTitle, string? metaDescription, string? metaKeywords)
    {
        // Validate and truncate if needed
        var title = string.IsNullOrWhiteSpace(metaTitle) ? null : Truncate(metaTitle, 60);
        var description = string.IsNullOrWhiteSpace(metaDescription) ? null : Truncate(metaDescription, 160);
        var keywords = string.IsNullOrWhiteSpace(metaKeywords) ? null : Truncate(metaKeywords, 255);

        return new SEOMetadata(title, description, keywords);
    }

    /// <summary>
    /// Creates empty SEO metadata.
    /// </summary>
    public static SEOMetadata Empty() => new(null, null, null);

    /// <summary>
    /// Creates SEO metadata from product/category name and description.
    /// </summary>
    public static SEOMetadata FromContent(string name, string? description)
    {
        var metaTitle = Truncate(name, 60);
        var metaDescription = string.IsNullOrWhiteSpace(description)
            ? Truncate(name, 160)
            : Truncate(description, 160);

        return new SEOMetadata(metaTitle, metaDescription, null);
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        text = text.Trim();
        if (text.Length <= maxLength)
            return text;

        return text[..(maxLength - 3)] + "...";
    }

    public bool IsEmpty() => string.IsNullOrWhiteSpace(MetaTitle)
                             && string.IsNullOrWhiteSpace(MetaDescription)
                             && string.IsNullOrWhiteSpace(MetaKeywords);

    public bool Equals(SEOMetadata? other)
    {
        if (other is null)
            return false;

        return MetaTitle == other.MetaTitle
               && MetaDescription == other.MetaDescription
               && MetaKeywords == other.MetaKeywords;
    }

    public override bool Equals(object? obj)
    {
        return obj is SEOMetadata metadata && Equals(metadata);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MetaTitle, MetaDescription, MetaKeywords);
    }

    public static bool operator ==(SEOMetadata? left, SEOMetadata? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(SEOMetadata? left, SEOMetadata? right)
    {
        return !(left == right);
    }
}
