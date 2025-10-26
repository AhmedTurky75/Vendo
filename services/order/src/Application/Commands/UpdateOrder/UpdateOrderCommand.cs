using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Application.DTOs;
using Vendo.OrderManagement.Domain.Enums;

namespace Vendo.OrderManagement.Application.Commands.UpdateOrder;

public class UpdateOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;
}
