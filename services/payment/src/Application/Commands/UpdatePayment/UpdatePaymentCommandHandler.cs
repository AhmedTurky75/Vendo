using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;
using Vendo.PaymentManagement.Domain.Enums;
using Vendo.PaymentManagement.Domain.Repositories;

namespace Vendo.PaymentManagement.Application.Commands.UpdatePayment;

/// <summary>
/// Handler for UpdatePaymentCommand.
/// </summary>
public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<UpdatePaymentCommandHandler> _logger;

    public UpdatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ILogger<UpdatePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating payment: {PaymentId}", request.Id);

            // Get existing payment
            var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Id);
                return Result<PaymentDto>.Failure($"Payment with ID {request.Id} not found");
            }

            // Parse payment method if provided
            PaymentMethod? paymentMethod = null;
            if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var parsedMethod))
                {
                    return Result<PaymentDto>.Failure($"Invalid payment method: {request.PaymentMethod}");
                }
                paymentMethod = parsedMethod;
            }

            // Update payment
            payment.Update(request.Amount, request.Currency, paymentMethod);

            // Update metadata if provided
            if (request.Metadata != null)
            {
                payment.UpdateMetadata(request.Metadata);
            }

            // Save changes
            _paymentRepository.Update(payment);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment updated successfully: {PaymentId}", payment.Id);

            // Map to DTO
            var paymentDto = MapToDto(payment);

            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot update payment: {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment: {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure("An error occurred while updating the payment");
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
