using ProductService.DAL.Models;

namespace ProductService.DAL.Repository.Abstractions;

public interface IProductMediaRepository
{
    Task<ProductMedia?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ProductMedia>> GetByProductIdAsync(Guid productId, CancellationToken ct);
    Task AddRangeAsync(IEnumerable<ProductMedia> media, CancellationToken ct);
    Task DeleteAsync(ProductMedia media, CancellationToken ct);
    Task DeleteByProductIdAsync(Guid productId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}