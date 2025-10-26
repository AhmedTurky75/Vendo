using FluentValidation;

namespace Vendo.OrderManagement.Application.Commands.UpdateOrder;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Order Id is required");
        RuleFor(x => x.CustomerName).NotEmpty().WithMessage("Customer name is required");
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().WithMessage("Valid email is required");
        RuleFor(x => x.ShippingAddress).NotEmpty().WithMessage("Shipping address is required");
        RuleFor(x => x.BillingAddress).NotEmpty().WithMessage("Billing address is required");
    }
}
