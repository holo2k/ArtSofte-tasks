namespace OrderService.Application.Dtos;

public record CreateOrderItemDto(Guid ProductId, int Quantity, decimal Price);