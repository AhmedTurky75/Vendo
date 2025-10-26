using MediatR;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;

namespace Vendo.PaymentManagement.Application.Queries.GetPayments;

/// <summary>
/// Query to get all payments with pagination.
/// </summary>
public sealed class GetPaymentsQuery : IRequest<Result<List<PaymentDto>>>
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}
