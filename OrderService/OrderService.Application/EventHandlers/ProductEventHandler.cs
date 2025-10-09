using System.Text.Json;
using Microsoft.Extensions.Logging;
using OrderService.Application.Dtos;
using OrderService.Application.Services.Abstractions;
using OrderService.Domain.Enums;
using OrderService.Domain.Models;
using OrderService.Domain.Repository.Abstractions;

namespace OrderService.Application.EventHandlers;

public class ProductEventHandler
{
    private readonly ILogger<ProductEventHandler> _logger;
    private readonly IOrderService _orders;
    private readonly IProductRepository _products;

    public ProductEventHandler(ILogger<ProductEventHandler> logger, IOrderService orders, IProductRepository products)
    {
        _logger = logger;
        _orders = orders;
        _products = products;
    }

    public async Task HandleAsync(string raw)
    {
        try
        {
            var ev = JsonSerializer.Deserialize<ProductEvent>(raw);
            if (ev == null) return;

            switch (ev.Type)
            {
                case "product.created":
                case "product.updated":
                    var existing = await _products.GetByIdAsync(ev.ProductId);
                    var newPrice = ev.NewPrice ?? existing?.Price ?? 0;
                    if (existing == null)
                    {
                        var snapshot = new ProductSnapshot(ev.ProductId, ev.SellerId, ev.Title, newPrice, true);
                        await _products.UpsertAsync(snapshot);
                    }
                    else
                    {
                        existing.Update(ev.Title, newPrice, true);
                        await _products.UpsertAsync(existing);
                    }

                    if (ev.Type == "product.updated")
                    {
                        var ordersWithProduct = await _orders.GetOrdersByProductAsync(ev.ProductId);
                        foreach (var order in ordersWithProduct.Where(o =>
                                     o.Status != OrderStatus.Paid && o.Status != OrderStatus.Delivered))
                        {
                            order.UpdateProductPrice(ev.ProductId, newPrice);
                            await _orders.SaveOrderAsync(order);
                            _logger.LogInformation("Order {OrderId} price updated for product {ProductId}", order.Id,
                                ev.ProductId);
                        }
                    }

                    break;

                case "product.deleted":
                    var snapshotToDelete = await _products.GetByIdAsync(ev.ProductId);
                    if (snapshotToDelete != null) await _products.DeleteAsync(ev.ProductId);

                    var orders = await _orders.GetOrdersByProductAsync(ev.ProductId);
                    foreach (var order in orders.Where(o => o.Status != OrderStatus.Delivered))
                    {
                        order.CancelDueToProductDeletion();
                        await _orders.UpdateStatus(order.Id, OrderStatus.Cancelled);
                        _logger.LogInformation("Order {OrderId} cancelled due to product deletion", order.Id);
                    }

                    break;

                default:
                    _logger.LogWarning("Unhandled product event type: {Type}", ev.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling product event");
        }
    }
}