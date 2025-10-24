namespace OrderService.Application.Messages;

public record ReleaseInventoryCommand(Guid OrderId, Guid ProductId, int Quantity);