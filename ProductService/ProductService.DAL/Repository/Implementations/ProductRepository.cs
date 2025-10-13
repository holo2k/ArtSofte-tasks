using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Dtos.Requests;
using ProductService.DAL.Models;
using ProductService.DAL.Persistence;
using ProductService.DAL.Repository.Abstractions;

namespace ProductService.DAL.Repository.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _db;

    public ProductRepository(ProductDbContext db)
    {
        _db = db;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Products
            .Include(p => p.Media)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Product>> GetPagedAsync(GetProductsRequest request, CancellationToken ct)
    {
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => EF.Functions.ILike(p.Title, $"%{request.Search}%"));
        if (request.Category.HasValue)
            query = query.Where(p => p.Category == request.Category);
        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        query = request.Sort switch
        {
            "price" => query.OrderBy(p => p.Price),
            "-price" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.AvgRating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "views" => query.OrderBy(p => p.Views),
            _ => query.OrderBy(p => p.Title)
        };

        return await query
            .Include(x => x.Media)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);
    }

    public async Task<Product> CreateWithMediaAsync(Product product, IEnumerable<ProductMedia>? media,
        CancellationToken ct)
    {
        await _db.Products.AddAsync(product, ct);

        if (media != null)
        {
            var productMedia = media.ToList();
            foreach (var m in productMedia)
            {
                m.Id = m.Id == Guid.Empty ? Guid.NewGuid() : m.Id;
                m.ProductId = product.Id;
            }

            await _db.ProductMedias.AddRangeAsync(productMedia, ct);
        }

        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<Product?> UpdateWithMediaAsync(Guid id, Product updated, IEnumerable<ProductMedia>? media,
        CancellationToken ct)
    {
        var existing = await _db.Products
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (existing == null) return null;

        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.Price = updated.Price;
        existing.Quantity = updated.Quantity;
        existing.Category = updated.Category;
        existing.UpdatedAt = DateTime.UtcNow;
        existing.IsActive = updated.IsActive;

        if (media != null)
        {
            var oldMedia = _db.ProductMedias.Where(m => m.ProductId == id);
            _db.ProductMedias.RemoveRange(oldMedia);

            var productMedia = media as ProductMedia[] ?? media.ToArray();
            foreach (var m in productMedia)
            {
                m.Id = m.Id == Guid.Empty ? Guid.NewGuid() : m.Id;
                m.ProductId = id;
            }

            await _db.ProductMedias.AddRangeAsync(productMedia, ct);
        }

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public Task UpdateAsync(Product product, CancellationToken ct)
    {
        _db.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product, CancellationToken ct)
    {
        _db.Products.Remove(product);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}