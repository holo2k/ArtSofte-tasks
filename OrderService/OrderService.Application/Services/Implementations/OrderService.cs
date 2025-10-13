using OrderService.Application.Clients;
using OrderService.Application.Dtos;
using OrderService.Application.Services.Abstractions;
using OrderService.Domain.Enums;
using OrderService.Domain.Models;
using OrderService.Domain.Repository.Abstractions;

namespace OrderService.Application.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IProductClient _productClient;
    private readonly IProductRepository _productRepo;
    private readonly IOrderRepository _repo;

    public OrderService(IOrderRepository repo, IProductRepository productRepo, IProductClient productClient)
    {
        _repo = repo;
        _productRepo = productRepo;
        _productClient = productClient;
    }

    public async Task<Guid> CreateOrder(Guid buyerId, Guid sellerId,
        List<(Guid productId, int qty, decimal price)> items)
    {
        if (items == null || items.Count == 0) throw new ArgumentException(nameof(items));

        var orderItems = new List<OrderItem>();
        foreach (var i in items)
        {
            var prod = await _productClient.GetProductByIdAsync(i.productId);
            if (prod == null)
                throw new InvalidOperationException($"Product {i.productId} not available");
            orderItems.Add(new OrderItem(i.productId, i.qty, prod.Price));
        }

        var order = new Order(buyerId, sellerId, orderItems);
        await _repo.AddAsync(order);
        return order.Id;
    }

    public async Task<List<Order>> GetOrdersByProductAsync(Guid productId)
    {
        return await _repo.GetByProductAsync(productId);
    }

    public async Task<Order?> GetOrder(Guid id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task<List<Order>> GetBuyerOrders(Guid buyerId)
    {
        return await _repo.GetByBuyerAsync(buyerId);
    }

    public async Task<List<Order>> GetSellerOrders(Guid sellerId)
    {
        return await _repo.GetBySellerAsync(sellerId);
    }

    public async Task UpdateStatus(Guid id, OrderStatus status)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order == null) throw new InvalidOperationException("Order not found");

        switch (status)
        {
            case OrderStatus.Paid:
                order.MarkAsPaid();
                break;
            case OrderStatus.Shipped:
                order.MarkAsShipped();
                break;
            case OrderStatus.Delivered:
                order.MarkAsDelivered();
                break;
            case OrderStatus.Pending:
            case OrderStatus.Cancelled:
            default: throw new ArgumentException("Invalid status");
        }

        await _repo.UpdateAsync(order);
    }

    public async Task SaveOrderAsync(Order order)
    {
        await _repo.UpdateAsync(order);
    }

    public async Task UpdateOrderItemsAsync(Guid orderId, List<UpdateItemDto> items)
    {
        var order = await _repo.GetByIdAsync(orderId);
        if (order == null) throw new InvalidOperationException("Order not found");
        if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot change items of a paid or delivered order");

        var newItems = new List<OrderItem>();
        foreach (var i in items)
        {
            var snap = await _productRepo.GetByIdAsync(i.ProductId);
            if (snap == null || !snap.IsActive)
                throw new InvalidOperationException($"Product {i.ProductId} is not available");

            newItems.Add(new OrderItem(i.ProductId, i.Quantity, snap.Price));
        }

        order.ReplaceItems(newItems);
        await _repo.UpdateAsync(order);
    }

    [Obsolete]
    public async Task<Guid> CreateOrderObsolete(Guid buyerId, Guid sellerId,
        List<(Guid productId, int qty, decimal price)> items)
    {
        if (items == null || items.Count == 0) throw new ArgumentException("items");

        var orderItems = new List<OrderItem>();
        foreach (var i in items)
        {
            var snap = await _productRepo.GetByIdAsync(i.productId);
            if (snap == null || !snap.IsActive)
                throw new InvalidOperationException($"Product {i.productId} is not available");

            var unitPrice = snap.Price;
            orderItems.Add(new OrderItem(i.productId, i.qty, unitPrice));
        }

        var order = new Order(buyerId, sellerId, orderItems);
        await _repo.AddAsync(order);
        return order.Id;
    }
}