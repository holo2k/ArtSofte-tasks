using ProductService.DAL.Dtos.Requests;
using ProductService.DAL.Models;
using ProductService.DAL.Requests;

namespace ProductService.Logic.Services.Abstractions;

public interface IProductService
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Product>> GetPagedAsync(GetProductsRequest request, CancellationToken ct);
    Task<Product> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<Product?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task IncrementViewsAsync(Guid id, int delta, CancellationToken ct);
}