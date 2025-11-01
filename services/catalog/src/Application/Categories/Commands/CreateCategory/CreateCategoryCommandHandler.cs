using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Categories.DTOs;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Categories.Commands.CreateCategory;

/// <summary>
/// Handler for creating a new category using DDD domain model.
/// </summary>
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating category: {Name} for tenant: {TenantId}", request.Name, request.TenantId);

        // Validate parent category if specified
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory == null)
            {
                return Result<CategoryDto>.Failure("Parent category not found");
            }

            if (parentCategory.TenantId != request.TenantId)
            {
                return Result<CategoryDto>.Failure("Parent category does not belong to this tenant");
            }
        }

        // Use factory method to create category
        var category = Domain.Entities.Category.Create(
            request.TenantId,
            request.Name,
            request.Description,
            request.ParentCategoryId,
            "system" // TODO: Get from auth context
        );

        // Set display order
        category.SetDisplayOrder(request.DisplayOrder, "system");

        // Set active status
        if (!request.IsActive)
        {
            category.Deactivate("system");
        }

        // Set image if provided
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            try
            {
                category.SetImage(request.ImageUrl, "system");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid image URL");
            }
        }

        // Add to repository
        await _unitOfWork.Categories.AddAsync(category, cancellationToken);

        // Save changes (will dispatch domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);

        // Map to DTO
        var categoryDto = new CategoryDto
        {
            Id = category.Id,
            TenantId = category.TenantId,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug.Value,
            ParentCategoryId = category.ParentCategoryId,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            ImageUrl = category.ImageUrl,
            ProductCount = 0,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };

        return Result<CategoryDto>.Success(categoryDto);
    }
}
