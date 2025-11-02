using Vendo.CatalogManagement.Domain.Entities;
using Vendo.CatalogManagement.Domain.Enums;
using Vendo.CatalogManagement.Domain.Events;
using Vendo.CatalogManagement.Domain.ValueObjects;

namespace Vendo.CatalogManagement.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var name = "Test Product";
        var description = "Test Description";
        var sku = SKU.Create("PROD-001")!;
        var price = Money.Create(99.99m)!;

        // Act
        var product = Product.Create(tenantId, categoryId, name, description, sku, price, "admin");

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.TenantId.Should().Be(tenantId);
        product.CategoryId.Should().Be(categoryId);
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.SKU.Value.Should().Be("PROD-001");
        product.Price.Amount.Should().Be(99.99m);
        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public void Create_ShouldRaiseProductCreatedEvent()
    {
        // Arrange
        var sku = SKU.Create("PROD-001")!;
        var price = Money.Create(99.99m)!;

        // Act
        var product = Product.Create(Guid.NewGuid(), Guid.NewGuid(), "Test", null, sku, price, "admin");

        // Assert
        product.DomainEvents.Should().ContainSingle();
        product.DomainEvents.First().Should().BeOfType<ProductCreatedEvent>();
    }

    [Fact]
    public void ChangePrice_ShouldUpdatePriceAndRaiseEvent()
    {
        // Arrange
        var product = CreateTestProduct();
        var newPrice = Money.Create(149.99m)!;

        // Act
        product.ChangePrice(newPrice, "admin");

        // Assert
        product.Price.Amount.Should().Be(149.99m);
        product.DomainEvents.Should().Contain(e => e is ProductPriceChangedEvent);
    }

    [Fact]
    public void UpdateStock_ShouldUpdateQuantityAndRaiseEvent()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        product.UpdateStock(50, "admin");

        // Assert
        product.StockQuantity.Should().Be(50);
        product.DomainEvents.Should().Contain(e => e is ProductStockChangedEvent);
    }

    [Fact]
    public void UpdateStock_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act & Assert
        var act = () => product.UpdateStock(-10, "admin");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Publish_WithValidProduct_ShouldChangeStatusToActive()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        product.Publish("admin");

        // Assert
        product.Status.Should().Be(ProductStatus.Active);
        product.DomainEvents.Should().Contain(e => e is ProductPublishedEvent);
    }

    [Fact]
    public void SetCompareAtPrice_GreaterThanPrice_ShouldSucceed()
    {
        // Arrange
        var product = CreateTestProduct(); // Price: 99.99
        var compareAtPrice = Money.Create(149.99m)!;

        // Act
        product.SetCompareAtPrice(compareAtPrice, "admin");

        // Assert
        product.CompareAtPrice.Should().NotBeNull();
        product.CompareAtPrice!.Amount.Should().Be(149.99m);
    }

    [Fact]
    public void SetCompareAtPrice_LessThanPrice_ShouldThrowException()
    {
        // Arrange
        var product = CreateTestProduct(); // Price: 99.99
        var compareAtPrice = Money.Create(49.99m)!;

        // Act & Assert
        var act = () => product.SetCompareAtPrice(compareAtPrice, "admin");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*greater than current price*");
    }

    [Fact]
    public void CalculateDiscountPercentage_WithCompareAtPrice_ShouldCalculateCorrectly()
    {
        // Arrange
        var product = CreateTestProduct(); // Price: 99.99
        product.SetCompareAtPrice(Money.Create(199.99m)!, "admin");

        // Act
        var discount = product.CalculateDiscountPercentage();

        // Assert
        discount.Should().NotBeNull();
        discount.Should().BeApproximately(50.00m, 0.01m); // ~50% discount
    }

    [Fact]
    public void IsLowStock_WhenBelowThreshold_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateTestProduct();
        product.SetLowStockThreshold(20, "admin");
        product.UpdateStock(15, "admin");

        // Act
        var isLow = product.IsLowStock();

        // Assert
        isLow.Should().BeTrue();
    }

    [Fact]
    public void IsOutOfStock_WhenZeroQuantity_ShouldReturnTrue()
    {
        // Arrange
        var product = CreateTestProduct();
        product.UpdateStock(0, "admin");

        // Act
        var isOut = product.IsOutOfStock();

        // Assert
        isOut.Should().BeTrue();
    }

    [Fact]
    public void AddTag_ShouldAddTagToCollection()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        product.AddTag("electronics", "admin");
        product.AddTag("gadget", "admin");

        // Assert
        product.Tags.Should().HaveCount(2);
        product.Tags.Should().Contain("electronics");
        product.Tags.Should().Contain("gadget");
    }

    [Fact]
    public void AddTag_Duplicate_ShouldNotAddAgain()
    {
        // Arrange
        var product = CreateTestProduct();

        // Act
        product.AddTag("electronics", "admin");
        product.AddTag("electronics", "admin");

        // Assert
        product.Tags.Should().HaveCount(1);
    }

    private static Product CreateTestProduct()
    {
        var sku = SKU.Create("TEST-001")!;
        var price = Money.Create(99.99m)!;
        return Product.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test Product",
            "Test Description",
            sku,
            price,
            "admin"
        );
    }
}
