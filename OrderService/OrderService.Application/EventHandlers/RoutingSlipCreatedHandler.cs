using MassTransit;
using OrderService.Application.Activities;

namespace OrderService.Application.EventHandlers;

public class RoutingSlipCreatedHandler : IConsumer<IRoutingSlipCreated>
{
    public Task Consume(ConsumeContext<IRoutingSlipCreated> context)
    {
        Console.WriteLine($"Routing Slip created: {context.Message.RoutingSlipId}");
        return Task.CompletedTask;
    }
}