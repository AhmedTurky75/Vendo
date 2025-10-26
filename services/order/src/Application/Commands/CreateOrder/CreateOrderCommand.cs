using MediatR;
using Vendo.OrderManagement.Application.Common;
using Vendo.OrderManagement.Application.DTOs;

namespace Vendo.OrderManagement.Application.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
