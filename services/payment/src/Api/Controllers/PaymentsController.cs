using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vendo.PaymentManagement.Application.Commands.CreatePayment;
using Vendo.PaymentManagement.Application.Commands.UpdatePayment;
using Vendo.PaymentManagement.Application.Commands.ProcessPayment;
using Vendo.PaymentManagement.Application.Commands.RefundPayment;
using Vendo.PaymentManagement.Application.Commands.DeletePayment;
using Vendo.PaymentManagement.Application.Queries.GetPayment;
using Vendo.PaymentManagement.Application.Queries.GetPayments;
using Vendo.PaymentManagement.Application.Queries.GetPaymentsByOrder;
using Vendo.PaymentManagement.Application.Queries.GetPaymentsByCustomer;
using Vendo.PaymentManagement.Application.DTOs;

namespace Vendo.PaymentManagement.Api.Controllers;

/// <summary>
/// API controller for managing payments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new payment.
    /// </summary>
    /// <param name="request">The payment creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created payment.</returns>
    /// <response code="201">Payment created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating payment for Order: {OrderId}", request.OrderId);

        var command = new CreatePaymentCommand
        {
            TenantId = request.TenantId,
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentMethod = request.PaymentMethod,
            Metadata = request.Metadata
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return CreatedAtAction(
            nameof(GetPayment),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Gets a payment by ID.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment details.</returns>
    /// <response code="200">Payment found.</response>
    /// <response code="404">Payment not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayment(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting payment: {PaymentId}", id);

        var query = new GetPaymentQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all payments with pagination.
    /// </summary>
    /// <param name="skip">Number of records to skip.</param>
    /// <param name="take">Number of records to take.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of payments.</returns>
    /// <response code="200">Payments retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting payments: Skip={Skip}, Take={Take}", skip, take);

        var query = new GetPaymentsQuery { Skip = skip, Take = take };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all payments for a specific order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of payments for the order.</returns>
    /// <response code="200">Payments retrieved successfully.</response>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentsByOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting payments for order: {OrderId}", orderId);

        var query = new GetPaymentsByOrderQuery { OrderId = orderId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all payments for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of payments for the customer.</returns>
    /// <response code="200">Payments retrieved successfully.</response>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentsByCustomer(
        [FromRoute] Guid customerId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting payments for customer: {CustomerId}", customerId);

        var query = new GetPaymentsByCustomerQuery { CustomerId = customerId };
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Processes a payment.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <param name="command">The process payment details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated payment.</returns>
    /// <response code="200">Payment processed successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Payment not found.</response>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessPayment(
        [FromRoute] Guid id,
        [FromBody] ProcessPaymentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment: {PaymentId}", id);

        command.Id = id;
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return NotFound(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Refunds a payment.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <param name="request">The refund details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated payment.</returns>
    /// <response code="200">Payment refunded successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Payment not found.</response>
    [HttpPost("{id:guid}/refund")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(
        [FromRoute] Guid id,
        [FromBody] RefundRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Refunding payment: {PaymentId}", id);

        var command = new RefundPaymentCommand
        {
            Id = id,
            RefundAmount = request.RefundAmount,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return NotFound(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates a payment.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <param name="command">The updated payment details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated payment.</returns>
    /// <response code="200">Payment updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Payment not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePayment(
        [FromRoute] Guid id,
        [FromBody] UpdatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating payment: {PaymentId}", id);

        command.Id = id;
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            {
                return NotFound(new { error = result.Error });
            }
            return BadRequest(new { error = result.Error, errors = result.Errors });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a payment.
    /// </summary>
    /// <param name="id">The payment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">Payment deleted successfully.</response>
    /// <response code="404">Payment not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayment(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting payment: {PaymentId}", id);

        var command = new DeletePaymentCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }
}
