using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;
using Vendo.PaymentManagement.Domain.Entities;
using Vendo.PaymentManagement.Domain.Enums;
using Vendo.PaymentManagement.Domain.Repositories;

namespace Vendo.PaymentManagement.Application.Commands.RefundPayment;

/// <summary>
/// Handler for RefundPaymentCommand.
/// </summary>
public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ITransactionRepository transactionRepository,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Refunding payment: {PaymentId}, Amount: {Amount}",
                request.Id, request.RefundAmount);

            // Get existing payment
            var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Id);
                return Result<PaymentDto>.Failure($"Payment with ID {request.Id} not found");
            }

            // Refund payment
            payment.Refund(request.RefundAmount, request.Reason);

            // Create transaction record
            var transaction = Transaction.Create(
                payment.Id,
                TransactionType.Refund,
                request.RefundAmount,
                payment.Status,
                null,
                $"Refunded: {request.Reason}"
            );

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            _paymentRepository.Update(payment);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment refunded successfully: {PaymentId}, Status: {Status}",
                payment.Id, payment.Status);

            // Map to DTO
            var paymentDto = MapToDto(payment);

            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot refund payment: {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid refund amount for payment: {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment: {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure("An error occurred while refunding the payment");
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
