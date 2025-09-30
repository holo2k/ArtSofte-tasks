using ProductService.DAL.Models;

namespace ProductService.Logic.Services.Abstractions;

public interface IFavoriteService
{
    Task<Favorite?> AddFavoriteAsync(Guid productId, CancellationToken ct);
    Task<bool> RemoveFavoriteAsync(Guid productId, CancellationToken ct);
    Task<IReadOnlyList<Favorite>> GetFavoritesAsync(Guid userId, int page, int pageSize, CancellationToken ct);
    Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken ct);
}