using MassTransit;
using OrderService.Application.Messages;

namespace OrderService.API.Consumers;

public class ReserveInventoryConsumer : IConsumer<ReserveInventoryCommand>
{
    public async Task Consume(ConsumeContext<ReserveInventoryCommand> context)
    {
        var msg = context.Message;
        if (msg.Quantity <= 5)
            await context.Publish(new InventoryReserved(msg.OrderId, msg.ProductId, msg.Quantity));
        else
            await context.Publish(new InventoryNotAvailable(msg.OrderId, msg.ProductId));
    }
}