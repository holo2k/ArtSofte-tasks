using MassTransit;
using OrderService.Application.Activities;
using OrderService.Application.Messages;
using OrderService.Application.Sagas;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CreateOrder, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => InventoryReserved, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => InventoryNotAvailable, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentAccepted, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentRejected, x => x.CorrelateById(m => m.Message.OrderId));

        State(() => Pending);
        State(() => InventoryReservedState);
        State(() => PaymentAcceptedState);
        State(() => Completed);
        State(() => Faulted);

        Initially(
            When(CreateOrder)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.BuyerId = context.Message.BuyerId;
                    context.Saga.ProductId = context.Message.ProductId;
                    context.Saga.Quantity = context.Message.Quantity;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.Created = DateTime.UtcNow;
                })
                .ThenAsync(async context =>
                {
                    var builder = new RoutingSlipBuilder(context.Message.OrderId);

                    var invArgs = new ReserveInventoryArguments(
                        context.Message.OrderId,
                        context.Message.ProductId,
                        context.Message.Quantity
                    );

                    var paymentArgs = new ChargePaymentArguments(
                        context.Message.OrderId,
                        context.Message.BuyerId,
                        context.Message.Amount
                    );

                    builder.AddActivity(
                        "ReserveInventory",
                        new Uri("queue:reserve-inventory_execute"),
                        invArgs
                    );

                    builder.AddActivity(
                        "ChargePayment",
                        new Uri("queue:charge-payment_execute"),
                        paymentArgs
                    );

                    builder.AddVariable("OrderId", context.Message.OrderId);
                    builder.AddVariable("ProductId", context.Message.ProductId);
                    builder.AddVariable("Quantity", context.Message.Quantity);
                    builder.AddVariable("BuyerId", context.Message.BuyerId);
                    builder.AddVariable("Amount", context.Message.Amount);

                    var routingSlip = builder.Build();
                    await context.Execute(routingSlip);
                })
                .TransitionTo(Pending)
        );

        During(Pending,
            When(InventoryReserved)
                .Then(context =>
                {
                    context.Saga.InventoryReserved = true;
                    Console.WriteLine($"Inventory reserved for order {context.Message.OrderId}");
                })
                .IfElse(context => context.Saga.PaymentAccepted,
                    thenBinder => thenBinder
                        .TransitionTo(Completed)
                        .Publish(context => new OrderCompleted(context.Saga.OrderId)),
                    elseBinder => elseBinder
                        .TransitionTo(InventoryReservedState)
                ),
            When(PaymentAccepted)
                .Then(context =>
                {
                    context.Saga.PaymentAccepted = true;
                    Console.WriteLine($"Payment accepted for order {context.Message.OrderId}");
                })
                .IfElse(context => context.Saga.InventoryReserved,
                    thenBinder => thenBinder
                        .TransitionTo(Completed)
                        .Publish(context => new OrderCompleted(context.Saga.OrderId)),
                    elseBinder => elseBinder
                        .TransitionTo(PaymentAcceptedState)
                ),
            When(InventoryNotAvailable)
                .Then(context => { Console.WriteLine($"Inventory not available for order {context.Message.OrderId}"); })
                .Publish(context => new OrderCancelled(context.Saga.OrderId, "Inventory not available"))
                .TransitionTo(Faulted),
            When(PaymentRejected)
                .Then(context => { Console.WriteLine($"Payment rejected for order {context.Message.OrderId}"); })
                .Publish(context => new OrderCancelled(context.Saga.OrderId, "Payment rejected"))
                .TransitionTo(Faulted)
        );

        During(InventoryReservedState,
            When(PaymentAccepted)
                .Then(context =>
                {
                    context.Saga.PaymentAccepted = true;
                    Console.WriteLine($"Payment completed for order {context.Message.OrderId}");
                })
                .TransitionTo(Completed)
                .Publish(context => new OrderCompleted(context.Saga.OrderId)),
            When(PaymentRejected)
                .ThenAsync(async context =>
                {
                    await context.Publish(new ReleaseInventoryCommand(
                        context.Saga.OrderId,
                        context.Saga.ProductId,
                        context.Saga.Quantity
                    ));
                })
                .Publish(context => new OrderCancelled(context.Saga.OrderId, "Payment rejected"))
                .TransitionTo(Faulted)
        );

        During(PaymentAcceptedState,
            When(InventoryReserved)
                .Then(context =>
                {
                    context.Saga.InventoryReserved = true;
                    Console.WriteLine($"Inventory completed for order {context.Message.OrderId}");
                })
                .TransitionTo(Completed)
                .Publish(context => new OrderCompleted(context.Saga.OrderId)),
            When(InventoryNotAvailable)
                .ThenAsync(async context =>
                {
                    await context.Publish(new RefundPaymentCommand(
                        context.Saga.OrderId,
                        context.Saga.BuyerId,
                        context.Saga.Amount
                    ));
                })
                .Publish(context => new OrderCancelled(context.Saga.OrderId, "Inventory not available"))
                .TransitionTo(Faulted)
        );

        During(Completed,
            Ignore(CreateOrder),
            Ignore(InventoryReserved),
            Ignore(PaymentAccepted)
        );

        During(Faulted,
            Ignore(CreateOrder),
            Ignore(InventoryReserved),
            Ignore(PaymentAccepted)
        );

        SetCompletedWhenFinalized();
    }

    public State Pending { get; private set; }
    public State InventoryReservedState { get; private set; }
    public State PaymentAcceptedState { get; private set; }
    public State Completed { get; private set; }
    public State Faulted { get; private set; }

    public Event<CreateOrderRequest> CreateOrder { get; private set; }
    public Event<InventoryReserved> InventoryReserved { get; private set; }
    public Event<InventoryNotAvailable> InventoryNotAvailable { get; private set; }
    public Event<PaymentAccepted> PaymentAccepted { get; private set; }
    public Event<PaymentRejected> PaymentRejected { get; private set; }
}