using MediatR;
using Vendo.Payment.Application.Common.Models;
using Vendo.Payment.Application.DTOs;

namespace Vendo.Payment.Application.Queries.GetTransactionHistory;

/// <summary>
/// Query to get transaction history for a payment.
/// </summary>
public sealed class GetTransactionHistoryQuery : IRequest<Result<List<TransactionDto>>>
{
    public Guid PaymentId { get; set; }
}
