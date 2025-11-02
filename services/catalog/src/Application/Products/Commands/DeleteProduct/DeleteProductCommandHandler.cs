using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.CatalogManagement.Application.Common;
using Vendo.CatalogManagement.Domain.Interfaces;

namespace Vendo.CatalogManagement.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Handler for deleting a product.
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId} for tenant: {TenantId}", request.Id, request.TenantId);

        // Get existing product
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<bool>.Failure("Product not found");
        }

        // Verify tenant ownership
        if (product.TenantId != request.TenantId)
        {
            return Result<bool>.Failure("Product does not belong to this tenant");
        }

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted successfully with ID: {ProductId}", request.Id);

        return Result<bool>.Success(true);
    }
}
