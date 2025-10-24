using MassTransit;
using OrderService.Application.Messages;

namespace OrderService.API.Consumers;

public class ChargePaymentConsumer : IConsumer<ChargePaymentCommand>
{
    public async Task Consume(ConsumeContext<ChargePaymentCommand> context)
    {
        var msg = context.Message;
        if (msg.Amount <= 1000)
            await context.Publish(new PaymentAccepted(msg.OrderId, msg.BuyerId, msg.Amount));
        else
            await context.Publish(new PaymentRejected(msg.OrderId, "Insufficient funds"));
    }
}