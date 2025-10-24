namespace OrderService.Application.Messages;

public record InventoryNotAvailable(Guid OrderId, Guid ProductId);