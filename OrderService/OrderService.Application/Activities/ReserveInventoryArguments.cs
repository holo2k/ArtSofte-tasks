namespace OrderService.Application.Activities;

public class ReserveInventoryArguments
{
    public ReserveInventoryArguments()
    {
    }

    public ReserveInventoryArguments(Guid orderId, Guid productId, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}