using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vendo.Order.Application.Commands.CancelOrder;
using Vendo.Order.Application.Commands.CreateOrder;
using Vendo.Order.Application.Commands.DeleteOrder;
using Vendo.Order.Application.Commands.UpdateOrder;
using Vendo.Order.Application.Commands.UpdateOrderStatus;
using Vendo.Order.Application.DTOs;
using Vendo.Order.Application.Queries.GetOrder;
using Vendo.Order.Application.Queries.GetOrders;
using Vendo.Order.Application.Queries.GetOrdersByCustomer;
using Vendo.Order.Application.Queries.GetOrdersByStatus;
using Vendo.Order.Domain.Enums;

namespace Vendo.Order.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand
        {
            TenantId = request.TenantId,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            SubTotal = request.SubTotal,
            TaxAmount = request.TaxAmount,
            ShippingCost = request.ShippingCost,
            DiscountAmount = request.DiscountAmount,
            TotalAmount = request.TotalAmount,
            Notes = request.Notes,
            Items = request.Items
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetOrder), new { id = result.Data!.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var query = new GetOrderQuery { Id = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetOrdersQuery { Page = page, PageSize = pageSize };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetOrdersByCustomer(Guid customerId)
    {
        var query = new GetOrdersByCustomerQuery { CustomerId = customerId };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetOrdersByStatus(OrderStatus status)
    {
        var query = new GetOrdersByStatusQuery { Status = status };
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderRequest request)
    {
        var command = new UpdateOrderCommand
        {
            Id = id,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            Status = request.Status,
            PaymentStatus = request.PaymentStatus,
            Notes = request.Notes,
            TrackingNumber = request.TrackingNumber
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var command = new UpdateOrderStatusCommand
        {
            Id = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var command = new CancelOrderCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var command = new DeleteOrderCommand { Id = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}
