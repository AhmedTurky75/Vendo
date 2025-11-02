namespace Vendo.CatalogManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing product images.
/// </summary>
public sealed class ProductImages : IEquatable<ProductImages>
{
    private const int MaxAdditionalImages = 10;

    public string? MainImageUrl { get; }
    public IReadOnlyList<string> AdditionalImageUrls { get; }

    private ProductImages(string? mainImageUrl, IReadOnlyList<string> additionalImageUrls)
    {
        MainImageUrl = mainImageUrl;
        AdditionalImageUrls = additionalImageUrls;
    }

    /// <summary>
    /// Creates a new ProductImages instance.
    /// </summary>
    public static ProductImages Create(string? mainImageUrl, IEnumerable<string>? additionalImageUrls = null)
    {
        var validatedMainUrl = ValidateUrl(mainImageUrl);

        var validatedAdditionalUrls = additionalImageUrls?
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(ValidateUrl)
            .Where(url => url != null)
            .Take(MaxAdditionalImages)
            .ToList() ?? new List<string>();

        return new ProductImages(validatedMainUrl, validatedAdditionalUrls!);
    }

    /// <summary>
    /// Creates empty product images.
    /// </summary>
    public static ProductImages Empty() => new(null, new List<string>());

    /// <summary>
    /// Adds an additional image.
    /// </summary>
    public ProductImages AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return this;

        var validatedUrl = ValidateUrl(imageUrl);
        if (validatedUrl == null)
            return this;

        if (AdditionalImageUrls.Count >= MaxAdditionalImages)
            return this;

        if (AdditionalImageUrls.Contains(validatedUrl))
            return this;

        var newUrls = new List<string>(AdditionalImageUrls) { validatedUrl };
        return new ProductImages(MainImageUrl, newUrls);
    }

    /// <summary>
    /// Removes an additional image.
    /// </summary>
    public ProductImages RemoveImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return this;

        var newUrls = AdditionalImageUrls.Where(url => url != imageUrl).ToList();
        return new ProductImages(MainImageUrl, newUrls);
    }

    /// <summary>
    /// Sets the main image.
    /// </summary>
    public ProductImages SetMainImage(string? imageUrl)
    {
        var validatedUrl = ValidateUrl(imageUrl);
        return new ProductImages(validatedUrl, AdditionalImageUrls);
    }

    /// <summary>
    /// Gets all image URLs (main + additional).
    /// </summary>
    public IReadOnlyList<string> GetAllImageUrls()
    {
        var allUrls = new List<string>();
        if (!string.IsNullOrWhiteSpace(MainImageUrl))
            allUrls.Add(MainImageUrl);
        allUrls.AddRange(AdditionalImageUrls);
        return allUrls;
    }

    private static string? ValidateUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        url = url.Trim();

        // Basic URL validation
        if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
        {
            if (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                return url;
        }

        return null;
    }

    public bool HasMainImage() => !string.IsNullOrWhiteSpace(MainImageUrl);
    public bool HasAdditionalImages() => AdditionalImageUrls.Any();
    public int TotalImageCount() => (HasMainImage() ? 1 : 0) + AdditionalImageUrls.Count;

    public bool Equals(ProductImages? other)
    {
        if (other is null)
            return false;

        return MainImageUrl == other.MainImageUrl
               && AdditionalImageUrls.SequenceEqual(other.AdditionalImageUrls);
    }

    public override bool Equals(object? obj)
    {
        return obj is ProductImages images && Equals(images);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(MainImageUrl);
        foreach (var url in AdditionalImageUrls)
        {
            hash.Add(url);
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(ProductImages? left, ProductImages? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ProductImages? left, ProductImages? right)
    {
        return !(left == right);
    }
}
