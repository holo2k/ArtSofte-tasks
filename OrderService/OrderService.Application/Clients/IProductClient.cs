using OrderService.Application.Dtos;

namespace OrderService.Application.Clients;

public interface IProductClient
{
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken ct = default);
}