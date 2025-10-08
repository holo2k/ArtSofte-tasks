namespace OrderService.Application.Dtos;

public record ProductEvent(string Type, Guid ProductId, Guid SellerId, string? Title, decimal? NewPrice = null);