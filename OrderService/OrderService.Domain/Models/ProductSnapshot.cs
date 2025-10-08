namespace OrderService.Domain.Models;

public class ProductSnapshot
{
    private ProductSnapshot()
    {
    }

    public ProductSnapshot(Guid id, Guid sellerId, string? title, decimal price, bool isActive)
    {
        Id = id;
        SellerId = sellerId;
        Title = title;
        Price = price;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; }
    public Guid SellerId { get; }
    public string? Title { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Update(string? title, decimal price, bool isActive)
    {
        Title = title;
        Price = price;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}