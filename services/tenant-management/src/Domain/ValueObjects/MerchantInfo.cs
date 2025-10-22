namespace Vendo.TenantManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing merchant contact and business information.
/// </summary>
public sealed class MerchantInfo : IEquatable<MerchantInfo>
{
    /// <summary>
    /// Gets the merchant's contact email address.
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// Gets the merchant's phone number.
    /// </summary>
    public string? Phone { get; }

    /// <summary>
    /// Gets the merchant's business name.
    /// </summary>
    public string? BusinessName { get; }

    /// <summary>
    /// Gets the merchant's business address.
    /// </summary>
    public string? Address { get; }

    /// <summary>
    /// Gets the merchant's city.
    /// </summary>
    public string? City { get; }

    /// <summary>
    /// Gets the merchant's state/province.
    /// </summary>
    public string? State { get; }

    /// <summary>
    /// Gets the merchant's postal/zip code.
    /// </summary>
    public string? PostalCode { get; }

    /// <summary>
    /// Gets the merchant's country.
    /// </summary>
    public string? Country { get; }

    private MerchantInfo(
        string email,
        string? phone,
        string? businessName,
        string? address,
        string? city,
        string? state,
        string? postalCode,
        string? country)
    {
        Email = email;
        Phone = phone;
        BusinessName = businessName;
        Address = address;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    /// <summary>
    /// Creates a new MerchantInfo instance.
    /// </summary>
    public static MerchantInfo Create(
        string email,
        string? phone = null,
        string? businessName = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        return new MerchantInfo(email, phone, businessName, address, city, state, postalCode, country);
    }

    public bool Equals(MerchantInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Email.Equals(other.Email, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is MerchantInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Email);
    }
}
