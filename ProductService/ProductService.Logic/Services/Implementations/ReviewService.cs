using AutoMapper;
using Core.EventBus;
using ProductService.DAL.Dtos.Requests;
using ProductService.DAL.Models;
using ProductService.DAL.Repository.Abstractions;
using ProductService.Logic.Services.Abstractions;

namespace ProductService.Logic.Services.Implementations;

public class ReviewService : IReviewService
{
    private readonly IEventPublisher _events;
    private readonly IMapper _mapper;
    private readonly IProductRepository _products;
    private readonly IReviewRepository _reviews;

    public ReviewService(IReviewRepository reviews, IProductRepository products, IEventPublisher events, IMapper mapper)
    {
        _reviews = reviews;
        _products = products;
        _events = events;
        _mapper = mapper;
    }

    public async Task<Review> AddReviewAsync(Guid productId, CreateReviewRequest request, CancellationToken ct)
    {
        var review = _mapper.Map<Review>(request);

        review.ProductId = productId;
        review.AuthorId = Guid.CreateVersion7(); //TODO AUTH

        var p = await _products.GetByIdAsync(review.ProductId, ct);
        if (p is null) throw new KeyNotFoundException("Product not found");

        await _reviews.AddAsync(review, ct);

        var reviews = await _reviews.GetByProductIdAsync(review.ProductId, 1, int.MaxValue, ct);
        var avg = reviews.Any() ? reviews.Average(r => r.Rating) : review.Rating;
        p.AvgRating = Math.Round((decimal)avg, 2);

        await _products.UpdateAsync(p, ct);
        await _reviews.SaveChangesAsync(ct);
        await _products.SaveChangesAsync(ct);

        await _events.PublishAsync("review.created", new { productId = review.ProductId, reviewId = review.Id },
            cancellationToken: ct);
        return review;
    }

    public Task<IReadOnlyList<Review>> GetByProductAsync(Guid productId, int page, int pageSize, CancellationToken ct)
    {
        return _reviews.GetByProductIdAsync(productId, page, pageSize, ct);
    }

    public async Task DeleteAsync(Guid reviewId, CancellationToken ct)
    {
        await _reviews.DeleteAsync(reviewId, ct);
        await _reviews.SaveChangesAsync(ct);
    }

    public async Task<Review?> ReplyAsync(Guid reviewId, string reply, CancellationToken ct)
    {
        var userId = Guid.CreateVersion7(); //TODO AUTH

        var r = await _reviews.GetByIdAsync(reviewId, ct);
        if (r is null) return null;

        var p = await _products.GetByIdAsync(r.ProductId, ct);
        if (p is null) return null;

        //if (p.SellerId != userId) throw new UnauthorizedAccessException("Only seller can reply"); TODO AUTH

        r.Reply = reply;
        await _reviews.UpdateAsync(r, ct);
        await _reviews.SaveChangesAsync(ct);

        await _events.PublishAsync("review.replied", new { productId = r.ProductId, reviewId = r.Id },
            cancellationToken: ct);
        return r;
    }
}