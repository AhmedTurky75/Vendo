using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;
using Vendo.Payment.Domain.Entities;
using Vendo.Payment.Domain.Enums;
using Vendo.Payment.Domain.Repositories;

namespace Vendo.Payment.Application.Commands.CreatePayment;

/// <summary>
/// Handler for CreatePaymentCommand.
/// </summary>
public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<CreatePaymentCommandHandler> _logger;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ILogger<CreatePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating payment for Order: {OrderId}", request.OrderId);

            // Parse payment method
            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var paymentMethod))
            {
                return Result<PaymentDto>.Failure($"Invalid payment method: {request.PaymentMethod}");
            }

            // Create payment entity
            var payment = Domain.Entities.Payment.Create(
                request.TenantId,
                request.OrderId,
                request.CustomerId,
                request.Amount,
                request.Currency,
                paymentMethod
            );

            // Update metadata if provided
            if (!string.IsNullOrWhiteSpace(request.Metadata))
            {
                payment.UpdateMetadata(request.Metadata);
            }

            // Save to repository
            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment created successfully with ID: {PaymentId}", payment.Id);

            // Map to DTO
            var paymentDto = MapToDto(payment);

            return Result<PaymentDto>.Success(paymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for Order: {OrderId}", request.OrderId);
            return Result<PaymentDto>.Failure("An error occurred while creating the payment");
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
