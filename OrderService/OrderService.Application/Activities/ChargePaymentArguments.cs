namespace OrderService.Application.Activities;

public class ChargePaymentArguments
{
    public ChargePaymentArguments()
    {
    }

    public ChargePaymentArguments(Guid orderId, Guid buyerId, decimal amount)
    {
        OrderId = orderId;
        BuyerId = buyerId;
        Amount = amount;
    }

    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal Amount { get; set; }
}