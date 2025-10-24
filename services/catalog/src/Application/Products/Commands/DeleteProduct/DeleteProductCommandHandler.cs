using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Catalog.Application.Common;
using Vendo.Catalog.Domain.Interfaces;

namespace Vendo.Catalog.Application.Products.Commands.DeleteProduct;

/// <summary>
/// Handler for deleting a product.
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId} for tenant: {TenantId}", request.Id, request.TenantId);

        // Get existing product
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result<bool>.Failure("Product not found");
        }

        // Verify tenant ownership
        if (product.TenantId != request.TenantId)
        {
            return Result<bool>.Failure("Product does not belong to this tenant");
        }

        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted successfully with ID: {ProductId}", request.Id);

        return Result<bool>.Success(true);
    }
}
