namespace OrderService.Application.Messages;

public record ReserveInventoryCommand(Guid OrderId, Guid ProductId, int Quantity);