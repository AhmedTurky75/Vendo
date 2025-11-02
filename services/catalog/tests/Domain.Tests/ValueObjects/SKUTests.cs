using Vendo.CatalogManagement.Domain.ValueObjects;

namespace Vendo.CatalogManagement.Domain.Tests.ValueObjects;

public class SKUTests
{
    [Theory]
    [InlineData("SKU-123")]
    [InlineData("PROD-ABC-001")]
    [InlineData("12345")]
    public void Create_WithValidSKU_ShouldSucceed(string skuValue)
    {
        // Arrange & Act
        var sku = SKU.Create(skuValue);

        // Assert
        sku.Should().NotBeNull();
        sku!.Value.Should().Be(skuValue.ToUpperInvariant());
    }

    [Theory]
    [InlineData("AB")] // Too short
    [InlineData("invalid-sku-with-very-long-name-that-exceeds-fifty-characters")] // Too long
    [InlineData("SKU WITH SPACES")]
    [InlineData("SKU@123")]
    public void Create_WithInvalidSKU_ShouldReturnNull(string invalidSku)
    {
        // Arrange & Act
        var sku = SKU.Create(invalidSku);

        // Assert
        sku.Should().BeNull();
    }

    [Fact]
    public void Create_NormalizesToUpperCase()
    {
        // Arrange & Act
        var sku = SKU.Create("sku-123");

        // Assert
        sku.Should().NotBeNull();
        sku!.Value.Should().Be("SKU-123");
    }

    [Fact]
    public void Generate_ShouldCreateValidSKU()
    {
        // Arrange & Act
        var sku = SKU.Generate("PROD");

        // Assert
        sku.Should().NotBeNull();
        sku.Value.Should().StartWith("PROD-");
        sku.Value.Length.Should().BeGreaterThan(5);
    }

    [Fact]
    public void Equals_TwoSKUsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var sku1 = SKU.Create("SKU-123")!;
        var sku2 = SKU.Create("sku-123")!; // lowercase

        // Act & Assert
        sku1.Should().Be(sku2); // Should be equal after normalization
    }
}
