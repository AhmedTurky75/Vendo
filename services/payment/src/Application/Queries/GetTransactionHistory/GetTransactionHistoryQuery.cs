using MediatR;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;

namespace Vendo.PaymentManagement.Application.Queries.GetTransactionHistory;

/// <summary>
/// Query to get transaction history for a payment.
/// </summary>
public sealed class GetTransactionHistoryQuery : IRequest<Result<List<TransactionDto>>>
{
    public Guid PaymentId { get; set; }
}
