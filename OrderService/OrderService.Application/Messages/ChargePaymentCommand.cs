namespace OrderService.Application.Messages;

public record ChargePaymentCommand(Guid OrderId, Guid BuyerId, decimal Amount);