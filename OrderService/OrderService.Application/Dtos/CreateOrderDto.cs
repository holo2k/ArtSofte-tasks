namespace OrderService.Application.Dtos;

public record CreateOrderDto(Guid BuyerId, Guid SellerId, List<CreateOrderItemDto> Items);