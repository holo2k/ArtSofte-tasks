using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Models;
using ProductService.DAL.Persistence;
using ProductService.DAL.Repository.Abstractions;

namespace ProductService.DAL.Repository.Implementations;

public class ProductMediaRepository : IProductMediaRepository
{
    private readonly ProductDbContext _db;

    public ProductMediaRepository(ProductDbContext db)
    {
        _db = db;
    }

    public async Task<ProductMedia?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.ProductMedias.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<IReadOnlyList<ProductMedia>> GetByProductIdAsync(Guid productId, CancellationToken ct)
    {
        return await _db.ProductMedias.Where(m => m.ProductId == productId).OrderBy(m => m.Order).ToListAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<ProductMedia> media, CancellationToken ct)
    {
        await _db.ProductMedias.AddRangeAsync(media, ct);
    }

    public Task DeleteAsync(ProductMedia media, CancellationToken ct)
    {
        _db.ProductMedias.Remove(media);
        return Task.CompletedTask;
    }

    public Task DeleteByProductIdAsync(Guid productId, CancellationToken ct)
    {
        var items = _db.ProductMedias.Where(m => m.ProductId == productId);
        _db.ProductMedias.RemoveRange(items);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }
}