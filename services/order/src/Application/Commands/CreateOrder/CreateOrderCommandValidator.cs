using FluentValidation;

namespace Vendo.Order.Application.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required");
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("CustomerId is required");
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Customer name is required");
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().WithMessage("Valid email is required");
        RuleFor(x => x.ShippingAddress).NotEmpty().WithMessage("Shipping address is required");
        RuleFor(x => x.BillingAddress).NotEmpty().WithMessage("Billing address is required");
        RuleFor(x => x.TotalAmount).GreaterThan(0).WithMessage("Total amount must be greater than 0");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have at least one item");
    }
}
