using OrderService.Domain.Enums;

namespace OrderService.Application.Dtos;

public record UpdateStatusDto(OrderStatus Status);