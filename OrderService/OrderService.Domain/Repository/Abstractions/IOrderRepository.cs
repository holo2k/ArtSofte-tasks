using OrderService.Domain.Models;

namespace OrderService.Domain.Repository.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<List<Order>> GetByBuyerAsync(Guid buyerId);
    Task<List<Order>> GetBySellerAsync(Guid sellerId);
    Task<List<Order>> GetByProductAsync(Guid productId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}