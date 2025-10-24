namespace OrderService.Application.Messages;

public record OrderCancelled(Guid OrderId, string Reason);