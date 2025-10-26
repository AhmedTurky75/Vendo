using MediatR;
using Microsoft.Extensions.Logging;
using Vendo.PaymentManagement.Application.Common.Models;
using Vendo.PaymentManagement.Application.DTOs;
using Vendo.PaymentManagement.Domain.Repositories;

namespace Vendo.PaymentManagement.Application.Queries.GetTransactionHistory;

/// <summary>
/// Handler for GetTransactionHistoryQuery.
/// </summary>
public class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, Result<List<TransactionDto>>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<GetTransactionHistoryQueryHandler> _logger;

    public GetTransactionHistoryQueryHandler(
        ITransactionRepository transactionRepository,
        ILogger<GetTransactionHistoryQueryHandler> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<Result<List<TransactionDto>>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting transaction history for payment: {PaymentId}", request.PaymentId);

            var transactions = await _transactionRepository.GetByPaymentAsync(request.PaymentId, cancellationToken);
            var transactionDtos = transactions.Select(MapToDto).ToList();

            return Result<List<TransactionDto>>.Success(transactionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction history for payment: {PaymentId}", request.PaymentId);
            return Result<List<TransactionDto>>.Failure("An error occurred while retrieving transaction history");
        }
    }

    private static TransactionDto MapToDto(Domain.Entities.Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            PaymentId = transaction.PaymentId,
            Type = transaction.Type.ToString(),
            Amount = transaction.Amount,
            Status = transaction.Status.ToString(),
            GatewayTransactionId = transaction.GatewayTransactionId,
            GatewayResponse = transaction.GatewayResponse,
            ProcessedAt = transaction.ProcessedAt
        };
    }
}
