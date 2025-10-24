using FluentValidation;

namespace Vendo.Order.Application.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Order Id is required");
    }
}
