namespace Vendo.TenantManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing store configuration settings.
/// </summary>
public sealed class StoreSettings
{
    /// <summary>
    /// Gets the store's currency code (ISO 4217).
    /// </summary>
    public string Currency { get; private set; }

    /// <summary>
    /// Gets the store's timezone (IANA timezone database).
    /// </summary>
    public string Timezone { get; private set; }

    /// <summary>
    /// Gets the store's language code (ISO 639-1).
    /// </summary>
    public string Language { get; private set; }

    /// <summary>
    /// Gets the default tax rate as a percentage (0-100).
    /// </summary>
    public decimal TaxRate { get; private set; }

    /// <summary>
    /// Gets whether the store charges tax.
    /// </summary>
    public bool TaxEnabled { get; private set; }

    /// <summary>
    /// Gets the store's primary brand color (hex format).
    /// </summary>
    public string? PrimaryColor { get; private set; }

    /// <summary>
    /// Gets the store's accent color for CTAs (hex format).
    /// </summary>
    public string? AccentColor { get; private set; }

    /// <summary>
    /// Gets the store's logo URL.
    /// </summary>
    public string? LogoUrl { get; private set; }

    private StoreSettings() { }

    private StoreSettings(
        string currency,
        string timezone,
        string language,
        decimal taxRate,
        bool taxEnabled,
        string? primaryColor,
        string? accentColor,
        string? logoUrl)
    {
        Currency = currency;
        Timezone = timezone;
        Language = language;
        TaxRate = taxRate;
        TaxEnabled = taxEnabled;
        PrimaryColor = primaryColor;
        AccentColor = accentColor;
        LogoUrl = logoUrl;
    }

    /// <summary>
    /// Creates default store settings for a new store.
    /// </summary>
    public static StoreSettings CreateDefault()
    {
        return new StoreSettings(
            currency: "USD",
            timezone: "America/New_York",
            language: "en",
            taxRate: 0,
            taxEnabled: false,
            primaryColor: null,
            accentColor: null,
            logoUrl: null
        );
    }

    /// <summary>
    /// Creates store settings with custom values.
    /// </summary>
    public static StoreSettings Create(
        string currency,
        string timezone,
        string language,
        decimal taxRate,
        bool taxEnabled,
        string? primaryColor = null,
        string? accentColor = null,
        string? logoUrl = null)
    {
        if (taxRate < 0 || taxRate > 100)
            throw new ArgumentException("Tax rate must be between 0 and 100", nameof(taxRate));

        return new StoreSettings(currency, timezone, language, taxRate, taxEnabled, primaryColor, accentColor, logoUrl);
    }

    /// <summary>
    /// Updates the currency setting.
    /// </summary>
    public void UpdateCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        Currency = currency;
    }

    /// <summary>
    /// Updates the tax settings.
    /// </summary>
    public void UpdateTaxSettings(decimal taxRate, bool taxEnabled)
    {
        if (taxRate < 0 || taxRate > 100)
            throw new ArgumentException("Tax rate must be between 0 and 100", nameof(taxRate));

        TaxRate = taxRate;
        TaxEnabled = taxEnabled;
    }

    /// <summary>
    /// Updates the branding colors.
    /// </summary>
    public void UpdateBranding(string? primaryColor, string? accentColor, string? logoUrl)
    {
        PrimaryColor = primaryColor;
        AccentColor = accentColor;
        LogoUrl = logoUrl;
    }
}
