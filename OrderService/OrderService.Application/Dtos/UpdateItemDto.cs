namespace OrderService.Application.Dtos;

public record UpdateItemDto(Guid ProductId, int Quantity);