using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vendo.PaymentManagement.Application.Queries.GetTransactionHistory;
using Vendo.PaymentManagement.Application.DTOs;

namespace Vendo.PaymentManagement.Api.Controllers;

/// <summary>
/// API controller for managing transactions.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(IMediator mediator, ILogger<TransactionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets transaction history for a payment.
    /// </summary>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of transactions for the payment.</returns>
    /// <response code="200">Transactions retrieved successfully.</response>
    [HttpGet("payment/{paymentId:guid}")]
    [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactionHistory(
        [FromRoute] Guid paymentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting transaction history for payment: {PaymentId}", paymentId);

        var query = new GetTransactionHistoryQuery { PaymentId = paymentId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
