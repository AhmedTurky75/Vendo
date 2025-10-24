using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Vendo.Order.Domain.Entities;
using Vendo.Order.Domain.Enums;

namespace Vendo.Order.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data into the Order database from JSON files.
/// </summary>
public class DataSeeder
{
    private readonly OrderDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private readonly string _dataPath;

    public DataSeeder(OrderDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;

        // Get the data directory path (goes up from bin/Debug/net9.0 to project root, then to Data/SeedData)
        var assemblyLocation = AppContext.BaseDirectory;
        _dataPath = Path.Combine(assemblyLocation, "..", "..", "..", "..", "Data", "SeedData");
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding for Order service...");

            await SeedOrdersAsync();

            _logger.LogInformation("Data seeding completed successfully for Order service.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the Order database.");
            throw;
        }
    }

    private async Task SeedOrdersAsync()
    {
        // Check if orders already exist
        if (await _context.Orders.AnyAsync())
        {
            _logger.LogInformation("Orders already exist. Skipping order seeding.");
            return;
        }

        var ordersFilePath = Path.Combine(_dataPath, "orders.json");

        if (!File.Exists(ordersFilePath))
        {
            _logger.LogWarning($"Orders seed file not found at {ordersFilePath}");
            return;
        }

        _logger.LogInformation($"Reading orders from {ordersFilePath}");
        var ordersJson = await File.ReadAllTextAsync(ordersFilePath);
        var orderDtos = JsonSerializer.Deserialize<List<OrderSeedDto>>(ordersJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (orderDtos == null || !orderDtos.Any())
        {
            _logger.LogWarning("No orders found in seed file.");
            return;
        }

        var orders = new List<Domain.Entities.Order>();
        foreach (var dto in orderDtos)
        {
            // Serialize addresses to JSON strings
            var shippingAddressJson = JsonSerializer.Serialize(dto.ShippingAddress);
            var billingAddressJson = JsonSerializer.Serialize(dto.BillingAddress);

            var order = new Domain.Entities.Order
            {
                Id = dto.Id,
                OrderId = dto.Id, // Using same GUID for both
                TenantId = dto.TenantId,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                ShippingAddress = shippingAddressJson,
                BillingAddress = billingAddressJson,
                OrderDate = dto.OrderDate,
                Status = dto.Status,
                PaymentStatus = dto.PaymentStatus,
                SubTotal = dto.SubTotal,
                TaxAmount = dto.TaxAmount,
                ShippingCost = dto.ShippingCost,
                DiscountAmount = dto.DiscountAmount,
                TotalAmount = dto.TotalAmount,
                Notes = dto.Notes ?? string.Empty,
                TrackingNumber = dto.TrackingNumber ?? string.Empty,
                CreatedAt = dto.OrderDate,
                UpdatedAt = dto.OrderDate
            };

            // Add order items
            if (dto.Items != null && dto.Items.Any())
            {
                foreach (var itemDto in dto.Items)
                {
                    var orderItem = new OrderItem
                    {
                        Id = itemDto.Id,
                        OrderId = dto.Id,
                        ProductId = itemDto.ProductId,
                        ProductName = itemDto.ProductName,
                        ProductSKU = itemDto.SKU,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        TotalPrice = itemDto.TotalPrice,
                        TenantId = dto.TenantId,
                        CreatedAt = dto.OrderDate,
                        UpdatedAt = dto.OrderDate
                    };

                    order.Items.Add(orderItem);
                }
            }

            orders.Add(order);
        }

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {orders.Count} orders successfully.");
    }
}

/// <summary>
/// DTO for deserializing order seed data from JSON.
/// </summary>
internal class OrderSeedDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CustomerId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public AddressDto ShippingAddress { get; set; } = new();
    public AddressDto BillingAddress { get; set; } = new();
    public List<OrderItemSeedDto>? Items { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingMethod { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }
}

/// <summary>
/// DTO for address within order.
/// </summary>
internal class AddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

/// <summary>
/// DTO for deserializing order item seed data from JSON.
/// </summary>
internal class OrderItemSeedDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ImageUrl { get; set; }
}
