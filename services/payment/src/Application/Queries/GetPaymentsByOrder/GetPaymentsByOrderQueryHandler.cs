using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;
using Vendo.Payment.Domain.Repositories;

namespace Vendo.Payment.Application.Queries.GetPaymentsByOrder;

/// <summary>
/// Handler for GetPaymentsByOrderQuery.
/// </summary>
public class GetPaymentsByOrderQueryHandler : IRequestHandler<GetPaymentsByOrderQuery, Result<List<PaymentDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPaymentsByOrderQueryHandler> _logger;

    public GetPaymentsByOrderQueryHandler(
        IPaymentRepository paymentRepository,
        ILogger<GetPaymentsByOrderQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<List<PaymentDto>>> Handle(GetPaymentsByOrderQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payments for order: {OrderId}", request.OrderId);

            var payments = await _paymentRepository.GetByOrderAsync(request.OrderId, cancellationToken);
            var paymentDtos = payments.Select(MapToDto).ToList();

            return Result<List<PaymentDto>>.Success(paymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments for order: {OrderId}", request.OrderId);
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
