using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;
using Vendo.PaymentManagement.Domain.Repositories;

namespace Vendo.PaymentManagement.Application.Queries.GetPayment;

/// <summary>
/// Handler for GetPaymentQuery.
/// </summary>
public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPaymentQueryHandler> _logger;

    public GetPaymentQueryHandler(
        IPaymentRepository paymentRepository,
        ILogger<GetPaymentQueryHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting payment: {PaymentId}", request.Id);

            var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Id);
                return Result<PaymentDto>.Failure($"Payment with ID {request.Id} not found");
            }

            var paymentDto = MapToDto(payment);
            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment: {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure("An error occurred while retrieving the payment");
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
