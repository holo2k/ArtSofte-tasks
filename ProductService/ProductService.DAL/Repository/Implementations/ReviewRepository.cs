using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Models;
using ProductService.DAL.Persistence;
using ProductService.DAL.Repository.Abstractions;

namespace ProductService.DAL.Repository.Implementations;

public class ReviewRepository : IReviewRepository
{
    private readonly ProductDbContext _db;

    public ReviewRepository(ProductDbContext db)
    {
        _db = db;
    }

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<Review>> GetByProductIdAsync(Guid productId, int page, int pageSize,
        CancellationToken ct)
    {
        return await _db.Reviews.Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Review review, CancellationToken ct)
    {
        await _db.Reviews.AddAsync(review, ct);
    }

    public Task UpdateAsync(Review review, CancellationToken ct)
    {
        _db.Reviews.Update(review);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid reviewId, CancellationToken ct)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct);

        if (review == null) throw new InvalidOperationException("No such ID");
        _db.Reviews.Remove(review);
    }
}