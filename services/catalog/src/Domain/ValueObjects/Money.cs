namespace Vendo.CatalogManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing monetary values.
/// Ensures money is always valid and provides business operations.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a new Money instance.
    /// </summary>
    /// <param name="amount">The monetary amount</param>
    /// <param name="currency">The currency code (e.g., USD, EUR)</param>
    /// <returns>Money instance if valid, otherwise null</returns>
    public static Money? Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            return null;

        if (string.IsNullOrWhiteSpace(currency))
            return null;

        return new Money(amount, currency.ToUpperInvariant());
    }

    /// <summary>
    /// Creates a zero money instance.
    /// </summary>
    public static Money Zero(string currency = "USD") => new(0, currency);

    /// <summary>
    /// Adds two money values.
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts money value.
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative");

        return new Money(result, Currency);
    }

    /// <summary>
    /// Multiplies money by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Checks if this money is greater than another.
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {Currency} and {other.Currency}");

        return Amount > other.Amount;
    }

    /// <summary>
    /// Checks if this money is less than another.
    /// </summary>
    public bool IsLessThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {Currency} and {other.Currency}");

        return Amount < other.Amount;
    }

    public bool Equals(Money? other)
    {
        if (other is null)
            return false;

        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj)
    {
        return obj is Money money && Equals(money);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    public override string ToString()
    {
        return $"{Amount:F2} {Currency}";
    }

    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right)
    {
        return !(left == right);
    }

    public static bool operator >(Money left, Money right)
    {
        return left.IsGreaterThan(right);
    }

    public static bool operator <(Money left, Money right)
    {
        return left.IsLessThan(right);
    }

    public static bool operator >=(Money left, Money right)
    {
        return left.IsGreaterThan(right) || left.Equals(right);
    }

    public static bool operator <=(Money left, Money right)
    {
        return left.IsLessThan(right) || left.Equals(right);
    }
}
