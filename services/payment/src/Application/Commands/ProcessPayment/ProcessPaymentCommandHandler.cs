using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;
using Vendo.Payment.Domain.Entities;
using Vendo.Payment.Domain.Enums;
using Vendo.Payment.Domain.Repositories;

namespace Vendo.Payment.Application.Commands.ProcessPayment;

/// <summary>
/// Handler for ProcessPaymentCommand.
/// </summary>
public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ITransactionRepository transactionRepository,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing payment: {PaymentId}", request.Id);

            // Get existing payment
            var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Id);
                return Result<PaymentDto>.Failure($"Payment with ID {request.Id} not found");
            }

            // Process payment
            payment.Process(request.TransactionId, request.GatewayResponse);
            _paymentRepository.Update(payment);

            // Complete or fail payment based on IsSuccess
            if (request.IsSuccess)
            {
                payment.Complete(request.GatewayResponse);

                // Create transaction record
                var transaction = Transaction.Create(
                    payment.Id,
                    TransactionType.Charge,
                    payment.Amount,
                    PaymentStatus.Completed,
                    request.TransactionId,
                    request.GatewayResponse
                );
                await _transactionRepository.AddAsync(transaction, cancellationToken);
            }
            else
            {
                payment.Fail(request.GatewayResponse);
            }

            _paymentRepository.Update(payment);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment processed successfully: {PaymentId}, Status: {Status}",
                payment.Id, payment.Status);

            // Map to DTO
            var paymentDto = MapToDto(payment);

            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot process payment: {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment: {PaymentId}", request.Id);
            return Result<PaymentDto>.Failure("An error occurred while processing the payment");
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
