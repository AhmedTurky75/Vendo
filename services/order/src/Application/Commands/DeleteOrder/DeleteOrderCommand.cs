using MediatR;
using Vendo.OrderManagement.Application.Common;

namespace Vendo.OrderManagement.Application.Commands.DeleteOrder;

public class DeleteOrderCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
