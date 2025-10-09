using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Models;
using OrderService.Domain.Repository.Abstractions;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repository.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetByBuyerAsync(Guid buyerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.BuyerId == buyerId)
            .ToListAsync();
    }

    public async Task<List<Order>> GetBySellerAsync(Guid sellerId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.SellerId == sellerId)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByProductAsync(Guid productId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.Items.Any(i => i.ProductId == productId))
            .ToListAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
}