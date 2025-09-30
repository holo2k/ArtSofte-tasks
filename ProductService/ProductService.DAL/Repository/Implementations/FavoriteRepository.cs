using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Models;
using ProductService.DAL.Persistence;
using ProductService.DAL.Repository.Abstractions;

namespace ProductService.DAL.Repository.Implementations;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly ProductDbContext _db;

    public FavoriteRepository(ProductDbContext db)
    {
        _db = db;
    }

    public async Task<Favorite?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Favorites.FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<Favorite?> GetByUserAndProductAsync(Guid userId, Guid productId, CancellationToken ct)
    {
        return await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId, ct);
    }

    public async Task<IReadOnlyList<Favorite>> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct)
    {
        return await _db.Favorites.Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Favorite favorite, CancellationToken ct)
    {
        await _db.Favorites.AddAsync(favorite, ct);
    }

    public Task RemoveAsync(Favorite favorite, CancellationToken ct)
    {
        _db.Favorites.Remove(favorite);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid productId, CancellationToken ct)
    {
        return await _db.Favorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}