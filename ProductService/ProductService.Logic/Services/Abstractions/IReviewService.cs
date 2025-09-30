using ProductService.DAL.Dtos.Requests;
using ProductService.DAL.Models;

namespace ProductService.Logic.Services.Abstractions;

public interface IReviewService
{
    Task<Review> AddReviewAsync(Guid productId, CreateReviewRequest request, CancellationToken ct);
    Task<Review?> ReplyAsync(Guid reviewId, string reply, CancellationToken ct);
    Task<IReadOnlyList<Review>> GetByProductAsync(Guid productId, int page, int pageSize, CancellationToken ct);
    Task DeleteAsync(Guid productId, CancellationToken ct);
}