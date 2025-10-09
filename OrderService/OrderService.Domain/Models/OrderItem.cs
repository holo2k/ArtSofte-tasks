namespace OrderService.Domain.Models;

public class OrderItem
{
    private OrderItem()
    {
    }

    public OrderItem(Guid productId, int quantity, decimal price)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }

    public Guid Id { get; }
    public Guid ProductId { get; }
    public int Quantity { get; }
    public decimal Price { get; private set; }

    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
    }
}