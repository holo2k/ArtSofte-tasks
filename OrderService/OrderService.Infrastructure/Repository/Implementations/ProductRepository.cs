using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Models;
using OrderService.Domain.Repository.Abstractions;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repository.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly OrderDbContext _db;

    public ProductRepository(OrderDbContext db)
    {
        _db = db;
    }

    public async Task<ProductSnapshot?> GetByIdAsync(Guid productId)
    {
        return await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task UpsertAsync(ProductSnapshot snapshot)
    {
        var exists = await _db.Products.FindAsync(snapshot.Id);
        if (exists == null)
        {
            await _db.Products.AddAsync(snapshot);
        }
        else
        {
            exists.Update(snapshot.Title, snapshot.Price, snapshot.IsActive);
            _db.Products.Update(exists);
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid productId)
    {
        var p = await _db.Products.FindAsync(productId);
        if (p != null)
        {
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
        }
    }
}