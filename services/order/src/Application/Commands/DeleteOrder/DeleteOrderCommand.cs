using MediatR;
using Vendo.Order.Application.Common;

namespace Vendo.Order.Application.Commands.DeleteOrder;

public class DeleteOrderCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
