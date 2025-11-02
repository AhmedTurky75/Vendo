using Vendo.CatalogManagement.Domain.Entities;
using Vendo.CatalogManagement.Domain.Events;

namespace Vendo.CatalogManagement.Domain.Tests.Entities;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCategory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "Electronics";
        var description = "Electronic products";

        // Act
        var category = Category.Create(tenantId, name, description, null, "admin");

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().NotBeEmpty();
        category.TenantId.Should().Be(tenantId);
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.IsActive.Should().BeTrue();
        category.Slug.Value.Should().Be("electronics");
    }

    [Fact]
    public void Create_ShouldRaiseCategoryCreatedEvent()
    {
        // Arrange & Act
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");

        // Assert
        category.DomainEvents.Should().ContainSingle();
        category.DomainEvents.First().Should().BeOfType<CategoryCreatedEvent>();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetActiveToTrue()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");
        category.Deactivate("admin");

        // Act
        category.Activate("admin");

        // Assert
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetActiveToFalse()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");

        // Act
        category.Deactivate("admin");

        // Assert
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ChangeParent_WithValidParentId_ShouldUpdate()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");
        var newParentId = Guid.NewGuid();

        // Act
        category.ChangeParent(newParentId, "admin");

        // Assert
        category.ParentCategoryId.Should().Be(newParentId);
    }

    [Fact]
    public void ChangeParent_ToSelf_ShouldThrowException()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");

        // Act & Assert
        var act = () => category.ChangeParent(category.Id, "admin");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be its own parent*");
    }

    [Fact]
    public void IsRootCategory_WithoutParent_ShouldReturnTrue()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");

        // Act
        var isRoot = category.IsRootCategory();

        // Assert
        isRoot.Should().BeTrue();
    }

    [Fact]
    public void IsRootCategory_WithParent_ShouldReturnFalse()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, Guid.NewGuid(), "admin");

        // Act
        var isRoot = category.IsRootCategory();

        // Assert
        isRoot.Should().BeFalse();
    }

    [Fact]
    public void UpdateInformation_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Old Name", "Old Description", null, "admin");

        // Act
        category.UpdateInformation("New Name", "New Description", "admin");

        // Assert
        category.Name.Should().Be("New Name");
        category.Description.Should().Be("New Description");
        category.Slug.Value.Should().Be("new-name");
    }

    [Fact]
    public void SetImage_WithValidUrl_ShouldUpdate()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");
        var imageUrl = "https://example.com/image.jpg";

        // Act
        category.SetImage(imageUrl, "admin");

        // Assert
        category.ImageUrl.Should().Be(imageUrl);
    }

    [Fact]
    public void SetImage_WithInvalidUrl_ShouldThrowException()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");

        // Act & Assert
        var act = () => category.SetImage("not-a-url", "admin");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetDisplayOrder_WithValidOrder_ShouldUpdate()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");

        // Act
        category.SetDisplayOrder(5, "admin");

        // Assert
        category.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void SetDisplayOrder_WithNegativeOrder_ShouldThrowException()
    {
        // Arrange
        var category = Category.Create(Guid.NewGuid(), "Test", null, null, "admin");

        // Act & Assert
        var act = () => category.SetDisplayOrder(-1, "admin");
        act.Should().Throw<ArgumentException>();
    }
}
