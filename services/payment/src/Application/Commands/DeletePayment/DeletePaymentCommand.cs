using MediatR;
using Vendo.Payment.Application.Common.Models;

namespace Vendo.Payment.Application.Commands.DeletePayment;

/// <summary>
/// Command to delete a payment.
/// </summary>
public sealed class DeletePaymentCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}
