namespace OrderService.Application.Messages;

public record CreateOrderRequest(Guid OrderId, Guid BuyerId, Guid ProductId, int Quantity, decimal Amount);