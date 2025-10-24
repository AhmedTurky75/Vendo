using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;
using Vendo.Payment.Domain.Repositories;

namespace Vendo.Payment.Application.Queries.GetPaymentsByCustomer;

/// <summary>
/// Handler for GetPaymentsByCustomerQuery.
/// </summary>
public class GetPaymentsByCustomerQueryHandler : IRequestHandler<GetPaymentsByCustomerQuery, Result<List<PaymentDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPaymentsByCustomerQueryHandler> _logger;

    public GetPaymentsByCustomerQueryHandler(
        IPaymentRepository paymentRepository,
        ILogger<GetPaymentsByCustomerQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<List<PaymentDto>>> Handle(GetPaymentsByCustomerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payments for customer: {CustomerId}", request.CustomerId);

            var payments = await _paymentRepository.GetByCustomerAsync(request.CustomerId, cancellationToken);
            var paymentDtos = payments.Select(MapToDto).ToList();

            return Result<List<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for customer: {CustomerId}", request.CustomerId);
            return Result<List<PaymentDto>>.Failure("An error occurred while retrieving payments");
        }
    }

    private static PaymentDto MapToDto(Domain.Entities.Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            TenantId = payment.TenantId,
            OrderId = payment.OrderId,
            CustomerId = payment.CustomerId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            PaymentMethod = payment.PaymentMethod.ToString(),
            Status = payment.Status.ToString(),
            PaymentDate = payment.PaymentDate,
            TransactionId = payment.TransactionId,
            GatewayResponse = payment.GatewayResponse,
            RefundAmount = payment.RefundAmount,
            RefundReason = payment.RefundReason,
            RefundDate = payment.RefundDate,
            Metadata = payment.Metadata,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
}
