namespace OrderService.Application.Messages;

public record InventoryReserved(Guid OrderId, Guid ProductId, int Quantity);