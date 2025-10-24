using MassTransit;
using OrderService.Application.Messages;

namespace OrderService.API.Consumers;

public class CreateOrderConsumer : IConsumer<CreateOrderRequest>
{
    private readonly IPublishEndpoint _publisher;

    public CreateOrderConsumer(IPublishEndpoint publisher)
    {
        _publisher = publisher;
    }

    public async Task Consume(ConsumeContext<CreateOrderRequest> context)
    {
        var msg = context.Message;

        await _publisher.Publish(new ReserveInventoryCommand(msg.OrderId, msg.ProductId, msg.Quantity));
        await _publisher.Publish(new ChargePaymentCommand(msg.OrderId, msg.BuyerId, msg.Amount));
    }
}