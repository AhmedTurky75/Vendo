using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;
using Vendo.Order.Domain.Enums;
using Vendo.Order.Domain.Repositories;

namespace Vendo.Order.Application.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null)
        {
            return Result<OrderDto>.Failure("Order not found");
        }

        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
        {
            return Result<OrderDto>.Failure($"Cannot cancel order with status {order.Status}");
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = "System";

        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        var orderDto = MapToDto(order);
        return Result<OrderDto>.Success(orderDto, "Order cancelled successfully");
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
