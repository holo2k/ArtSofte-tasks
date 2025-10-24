namespace OrderService.Application.Messages;

public record PaymentRejected(Guid OrderId, string Reason);