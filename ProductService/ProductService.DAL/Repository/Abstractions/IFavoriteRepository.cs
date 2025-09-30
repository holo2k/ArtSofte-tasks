using ProductService.DAL.Models;

namespace ProductService.DAL.Repository.Abstractions;

public interface IFavoriteRepository
{
    Task<Favorite?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Favorite?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken ct);
    Task<IReadOnlyList<Favorite>> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct);
    Task AddAsync(Favorite favorite, CancellationToken ct);
    Task RemoveAsync(Favorite favorite, CancellationToken ct);
    Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}