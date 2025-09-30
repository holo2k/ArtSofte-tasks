using Core.EventBus;
using ProductService.DAL.Models;
using ProductService.DAL.Repository.Abstractions;
using ProductService.Logic.Services.Abstractions;

namespace ProductService.Logic.Services.Implementations;

public class FavoriteService : IFavoriteService
{
    private readonly IEventPublisher _events;
    private readonly IFavoriteRepository _fav;

    public FavoriteService(IFavoriteRepository fav, IEventPublisher events)
    {
        _fav = fav;
        _events = events;
    }

    public Task<IReadOnlyList<Favorite>> GetFavoritesAsync(Guid userId, int page, int pageSize, CancellationToken ct)
    {
        return _fav.GetByUserAsync(userId, page, pageSize, ct);
    }

    public Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken ct)
    {
        return _fav.ExistsAsync(userId, productId, ct);
    }

    public async Task<bool> RemoveFavoriteAsync(Guid productId, CancellationToken ct)
    {
        var userId = Guid.CreateVersion7(); //TODO AUTH

        var existing = await _fav.GetByUserAndProductAsync(userId, productId, ct);
        if (existing is null) return false;

        await _fav.RemoveAsync(existing, ct);
        await _fav.SaveChangesAsync(ct);

        await _events.PublishAsync("favorite.removed", new { userId, productId }, cancellationToken: ct);
        return true;
    }

    public async Task<Favorite?> AddFavoriteAsync(Guid productId, CancellationToken ct)
    {
        var userId = Guid.CreateVersion7(); //TODO AUTH

        if (await _fav.ExistsAsync(userId, productId, ct)) return null;

        var f = new Favorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };

        await _fav.AddAsync(f, ct);
        await _fav.SaveChangesAsync(ct);

        await _events.PublishAsync("favorite.added", new { userId, productId }, cancellationToken: ct);
        return f;
    }
}