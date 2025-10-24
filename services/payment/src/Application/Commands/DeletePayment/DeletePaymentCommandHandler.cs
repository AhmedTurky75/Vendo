using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Domain.Repositories;

namespace Vendo.Payment.Application.Commands.DeletePayment;

/// <summary>
/// Handler for DeletePaymentCommand.
/// </summary>
public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<DeletePaymentCommandHandler> _logger;

    public DeletePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ILogger<DeletePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting payment: {PaymentId}", request.Id);

            // Get existing payment
            var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found: {PaymentId}", request.Id);
                return Result.Failure($"Payment with ID {request.Id} not found");
            }

            // Delete payment
            _paymentRepository.Delete(payment);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment deleted successfully: {PaymentId}", request.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment: {PaymentId}", request.Id);
            return Result.Failure("An error occurred while deleting the payment");
        }
    }
}
