using OrderService.Application.Dtos;
using OrderService.Domain.Enums;
using OrderService.Domain.Models;

namespace OrderService.Application.Services.Abstractions;

public interface IOrderService
{
    Task<Guid> CreateOrder(Guid buyerId, Guid sellerId, List<(Guid productId, int qty, decimal price)> items);
    Task<List<Order>> GetOrdersByProductAsync(Guid productId);
    Task<Order?> GetOrder(Guid id);
    Task<List<Order>> GetBuyerOrders(Guid buyerId);
    Task<List<Order>> GetSellerOrders(Guid sellerId);
    Task UpdateStatus(Guid id, OrderStatus status);
    Task SaveOrderAsync(Order order);
    Task UpdateOrderItemsAsync(Guid orderId, List<UpdateItemDto> items);
}