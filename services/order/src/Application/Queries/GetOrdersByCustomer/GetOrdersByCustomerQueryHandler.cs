using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;
using Vendo.Order.Domain.Repositories;

namespace Vendo.Order.Application.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, Result<List<OrderDto>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<List<OrderDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerAsync(request.CustomerId, cancellationToken);
        var orderDtos = orders.Select(MapToDto).ToList();
        return Result<List<OrderDto>>.Success(orderDtos);
    }

    private static OrderDto MapToDto(Domain.Entities.Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderId = order.OrderId,
            TenantId = order.TenantId,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            ShippingAddress = order.ShippingAddress,
            BillingAddress = order.BillingAddress,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingCost,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,
            Notes = order.Notes,
            TrackingNumber = order.TrackingNumber,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductSKU = i.ProductSKU,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                TenantId = i.TenantId
            }).ToList()
        };
    }
}
