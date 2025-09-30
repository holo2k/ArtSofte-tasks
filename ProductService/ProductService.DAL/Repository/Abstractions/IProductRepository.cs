using ProductService.DAL.Dtos.Requests;
using ProductService.DAL.Models;

namespace ProductService.DAL.Repository.Abstractions;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<Product>> GetPagedAsync(
        GetProductsRequest request,
        CancellationToken ct);

    Task UpdateAsync(Product product, CancellationToken ct);
    Task DeleteAsync(Product product, CancellationToken ct);
    Task<Product> CreateWithMediaAsync(Product product, IEnumerable<ProductMedia>? media, CancellationToken ct);

    Task<Product?> UpdateWithMediaAsync(Guid id, Product updated, IEnumerable<ProductMedia>? media,
        CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}