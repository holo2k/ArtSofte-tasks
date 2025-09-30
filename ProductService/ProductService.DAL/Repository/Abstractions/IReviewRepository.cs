using ProductService.DAL.Models;

namespace ProductService.DAL.Repository.Abstractions;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Review>> GetByProductIdAsync(Guid productId, int page, int pageSize, CancellationToken ct);
    Task AddAsync(Review review, CancellationToken ct);
    Task UpdateAsync(Review review, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}