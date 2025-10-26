using MediatR;
using Vendo.PaymentManagement.Application.Common.Models;

namespace Vendo.PaymentManagement.Application.Commands.DeletePayment;

/// <summary>
/// Command to delete a payment.
/// </summary>
public sealed class DeletePaymentCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}
