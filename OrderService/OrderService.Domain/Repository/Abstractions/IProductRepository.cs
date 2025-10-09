using OrderService.Domain.Models;

namespace OrderService.Domain.Repository.Abstractions;

public interface IProductRepository
{
    Task<ProductSnapshot?> GetByIdAsync(Guid productId);
    Task UpsertAsync(ProductSnapshot snapshot);
    Task DeleteAsync(Guid productId);
}