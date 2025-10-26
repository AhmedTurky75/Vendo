using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vendo.PaymentManagement.Domain.Entities;
using Vendo.PaymentManagement.Domain.Enums;

namespace Vendo.PaymentManagement.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data into the Payment database from JSON files.
/// </summary>
public class DataSeeder
{
    private readonly PaymentDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private readonly string _dataPath;

    public DataSeeder(PaymentDbContext context, ILogger<DataSeeder> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;

        // Get the data directory path from configuration
        var configuredPath = configuration["SeedDataSettings:DataPath"] ?? "Infrastructure/Persistence/SeedData";
        var baseDirectory = AppContext.BaseDirectory;

        // Navigate up from bin/Debug/net9.0 to the src folder, then to the configured path
        var srcRoot = Path.Combine(baseDirectory, "..", "..", "..", "..");
        _dataPath = Path.GetFullPath(Path.Combine(srcRoot, configuredPath));
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting data seeding for Payment service...");

            await SeedPaymentsAsync();

            _logger.LogInformation("Data seeding completed successfully for Payment service.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the Payment database.");
            throw;
        }
    }

    private async Task SeedPaymentsAsync()
    {
        // Check if payments already exist
        if (await _context.Payments.AnyAsync())
        {
            _logger.LogInformation("Payments already exist. Skipping payment seeding.");
            return;
        }

        var paymentsFilePath = Path.Combine(_dataPath, "payments.json");

        if (!File.Exists(paymentsFilePath))
        {
            _logger.LogWarning($"Payments seed file not found at {paymentsFilePath}");
            return;
        }

        _logger.LogInformation($"Reading payments from {paymentsFilePath}");
        var paymentsJson = await File.ReadAllTextAsync(paymentsFilePath);
        var paymentDtos = JsonSerializer.Deserialize<List<PaymentSeedDto>>(paymentsJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

        if (paymentDtos == null || !paymentDtos.Any())
        {
            _logger.LogWarning("No payments found in seed file.");
            return;
        }

        // We need to use reflection to set private properties on Payment entity
        // because it has a factory method pattern with private setters
        var payments = new List<Domain.Entities.Payment>();
        foreach (var dto in paymentDtos)
        {
            // Create payment using factory method
            var payment = Domain.Entities.Payment.Create(
                dto.TenantId,
                dto.OrderId,
                dto.CustomerId,
                dto.Amount,
                dto.Currency,
                dto.PaymentMethod
            );

            // Use reflection to set properties that don't have public setters
            SetPrivateProperty(payment, "Id", dto.Id);
            SetPrivateProperty(payment, "Status", dto.Status);
            SetPrivateProperty(payment, "CreatedAt", dto.CreatedAt);
            SetPrivateProperty(payment, "UpdatedAt", dto.CreatedAt);

            if (dto.PaidAt.HasValue)
            {
                SetPrivateProperty(payment, "PaymentDate", dto.PaidAt);
            }

            if (!string.IsNullOrEmpty(dto.GatewayTransactionId))
            {
                SetPrivateProperty(payment, "TransactionId", dto.GatewayTransactionId);
            }

            if (!string.IsNullOrEmpty(dto.GatewayPaymentMethodId))
            {
                var metadata = JsonSerializer.Serialize(new
                {
                    paymentMethodId = dto.GatewayPaymentMethodId,
                    cardLast4 = dto.CardLast4,
                    cardBrand = dto.CardBrand,
                    cardExpiryMonth = dto.CardExpiryMonth,
                    cardExpiryYear = dto.CardExpiryYear,
                    billingEmail = dto.BillingEmail,
                    billingName = dto.BillingName,
                    billingAddress = dto.BillingAddress,
                    paymentGateway = dto.PaymentGateway,
                    description = dto.Description,
                    failureReason = dto.FailureReason,
                    failureCode = dto.FailureCode,
                    originalMetadata = dto.Metadata
                });
                SetPrivateProperty(payment, "Metadata", metadata);
            }

            if (dto.RefundAmount > 0)
            {
                SetPrivateProperty(payment, "RefundAmount", dto.RefundAmount);
                SetPrivateProperty(payment, "RefundReason", dto.RefundReason);
                SetPrivateProperty(payment, "RefundDate", dto.RefundDate);
            }

            payments.Add(payment);
        }

        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {payments.Count} payments successfully.");
    }

    private static void SetPrivateProperty<T>(object obj, string propertyName, T value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property != null)
        {
            property.SetValue(obj, value);
        }
    }
}

/// <summary>
/// DTO for deserializing payment seed data from JSON.
/// </summary>
internal class PaymentSeedDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentGateway { get; set; }
    public PaymentStatus Status { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? GatewayPaymentMethodId { get; set; }
    public string? CardLast4 { get; set; }
    public string? CardBrand { get; set; }
    public int? CardExpiryMonth { get; set; }
    public int? CardExpiryYear { get; set; }
    public string? BillingEmail { get; set; }
    public string? BillingName { get; set; }
    public object? BillingAddress { get; set; }
    public string? Description { get; set; }
    public object? Metadata { get; set; }
    public decimal RefundAmount { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? RefundDate { get; set; }
    public string? FailureReason { get; set; }
    public string? FailureCode { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
