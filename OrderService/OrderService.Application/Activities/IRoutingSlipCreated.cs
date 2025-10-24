namespace OrderService.Application.Activities;

public interface IRoutingSlipCreated
{
    Guid RoutingSlipId { get; }
    DateTime CreatedAt { get; }
}