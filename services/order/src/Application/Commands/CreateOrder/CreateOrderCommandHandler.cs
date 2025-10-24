using MediatR;
using Vendo.Order.Application.Common;
using Vendo.Order.Application.DTOs;
using Vendo.Order.Domain.Entities;
using Vendo.Order.Domain.Enums;
using Vendo.Order.Domain.Repositories;

namespace Vendo.Order.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Domain.Entities.Order
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            TenantId = request.TenantId,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            SubTotal = request.SubTotal,
            TaxAmount = request.TaxAmount,
            ShippingCost = request.ShippingCost,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = request.TotalAmount,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "System",
            UpdatedBy = "System"
        };

        // Add order items
        foreach (var itemRequest in request.Items)
        {
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = itemRequest.ProductId,
                ProductName = itemRequest.ProductName,
                ProductSKU = itemRequest.ProductSKU,
                Quantity = itemRequest.Quantity,
                UnitPrice = itemRequest.UnitPrice,
                TotalPrice = itemRequest.TotalPrice,
                TenantId = request.TenantId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "System",
                UpdatedBy = "System"
            };
            order.Items.Add(orderItem);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        var orderDto = MapToDto(order);
        return Result<OrderDto>.Success(orderDto, "Order created successfully");
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
