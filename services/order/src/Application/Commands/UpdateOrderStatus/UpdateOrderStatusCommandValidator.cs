using FluentValidation;

namespace Vendo.Order.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Order Id is required");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Valid order status is required");
    }
}
