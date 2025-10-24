using Vendo.Order.Domain.Common;
using Vendo.Order.Domain.Enums;

namespace Vendo.Order.Domain.Entities;

public class Order : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string TrackingNumber { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
