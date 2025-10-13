using AutoMapper;
using Core.EventBus;
using ProductService.DAL.Dtos.Requests;
using ProductService.DAL.Models;
using ProductService.DAL.Repository.Abstractions;
using ProductService.DAL.Requests;
using ProductService.Logic.Services.Abstractions;

namespace ProductService.Logic.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IEventPublisher _events;
    private readonly IMapper _mapper;
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository, IEventPublisher events, IMapper mapper)
    {
        _repository = repository;
        _events = events;
        _mapper = mapper;
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _repository.GetByIdAsync(id, ct);
    }

    public Task<IReadOnlyList<Product>> GetPagedAsync(GetProductsRequest request, CancellationToken ct)
    {
        return _repository.GetPagedAsync(request, ct);
    }

    public async Task<Product> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var product = _mapper.Map<Product>(request);
        product.Id = Guid.CreateVersion7();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        product.SellerId = Guid.CreateVersion7(); //TODO AUTH

        product.Media = request.Media?.Select(m =>
        {
            var pm = _mapper.Map<ProductMedia>(m);
            pm.Id = Guid.CreateVersion7();
            pm.ProductId = product.Id;
            return pm;
        }).ToList() ?? new List<ProductMedia>();

        var created = await _repository.CreateWithMediaAsync(product, product.Media, ct);

        await _events.PublishAsync("product.created", new
        {
            productId = created.Id,
            sellerId = created.SellerId,
            title = created.Title
        }, cancellationToken: ct);

        return created;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        if (product == null) return false;

        await _repository.DeleteAsync(product, ct);
        await _repository.SaveChangesAsync(ct);

        await _events.PublishAsync("product.deleted", new
        {
            productId = product.Id,
            sellerId = product.SellerId
        }, cancellationToken: ct);

        return true;
    }

    public async Task IncrementViewsAsync(Guid id, int delta, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        if (product == null) return;

        product.Views += delta;
        product.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(product, ct);
        await _repository.SaveChangesAsync(ct);

        await _events.PublishAsync("product.viewed", new { productId = id, delta }, cancellationToken: ct);
    }

    public async Task<Product?> UpdateAsync(Guid id, UpdateProductRequest request,
        CancellationToken ct)
    {
        var updated = _mapper.Map<Product>(request);
        updated.Id = id;
        updated.UpdatedAt = DateTime.UtcNow;
        updated.IsActive = request.IsActive;
        updated.SellerId = Guid.CreateVersion7(); // TODO AUTH

        updated.Media = request.Media?.Select(m =>
        {
            var pm = _mapper.Map<ProductMedia>(m);
            pm.Id = Guid.NewGuid();
            pm.ProductId = updated.Id;
            return pm;
        }).ToList() ?? new List<ProductMedia>();

        var updatedProduct = await _repository.UpdateWithMediaAsync(id, updated, updated.Media, ct);
        if (updatedProduct == null) return null;

        await _events.PublishAsync("product.updated", new
        {
            productId = updatedProduct.Id,
            sellerId = updatedProduct.SellerId
        }, cancellationToken: ct);

        return updatedProduct;
    }
}