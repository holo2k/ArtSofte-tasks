namespace OrderService.Application.Messages;

public record PaymentAccepted(Guid OrderId, Guid BuyerId, decimal Amount);