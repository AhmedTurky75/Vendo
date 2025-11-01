using Vendo.CatalogManagement.Domain.ValueObjects;

namespace Vendo.CatalogManagement.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Arrange & Act
        var money = Money.Create(100.50m, "USD");

        // Assert
        money.Should().NotBeNull();
        money!.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldReturnNull()
    {
        // Arrange & Act
        var money = Money.Create(-10m);

        // Assert
        money.Should().BeNull();
    }

    [Fact]
    public void Add_TwoMoneyWithSameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD")!;
        var money2 = Money.Create(50m, "USD")!;

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_TwoMoneyWithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD")!;
        var money2 = Money.Create(50m, "EUR")!;

        // Act & Assert
        var act = () => money1.Add(money2);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Equals_TwoMoneyWithSameValues_ShouldBeEqual()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD")!;
        var money2 = Money.Create(100m, "USD")!;

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void IsGreaterThan_ShouldCompareCorrectly()
    {
        // Arrange
        var larger = Money.Create(100m)!;
        var smaller = Money.Create(50m)!;

        // Act & Assert
        larger.IsGreaterThan(smaller).Should().BeTrue();
        (larger > smaller).Should().BeTrue();
        smaller.IsLessThan(larger).Should().BeTrue();
    }
}
