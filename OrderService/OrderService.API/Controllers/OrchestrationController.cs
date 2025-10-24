using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Activities;
using OrderService.Application.Messages;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/orchestration")]
public class OrchestrationController : ControllerBase
{
    private readonly IBus _bus;
    private readonly IPublishEndpoint _publisher;

    public OrchestrationController(IPublishEndpoint publisher, IBus bus)
    {
        _publisher = publisher;
        _bus = bus;
    }

    /// <summary>
    ///     Запускает процесс создания заказа через <b>Saga (Coordinator)</b>.
    /// </summary>
    /// <param name="req">Данные заказа.</param>
    /// <returns>HTTP 202 (Accepted), если сообщение успешно отправлено в шину.</returns>
    /// <response code="202">Сообщение успешно опубликовано в Saga.</response>
    /// <response code="400">Неверные данные заказа.</response>
    [HttpPost("coordinator")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<IActionResult> Coordinator([FromBody] CreateOrderRequest req)
    {
        await _publisher.Publish(req);
        return Accepted(new { message = "Order request sent to saga coordinator" });
    }

    /// <summary>
    ///     Запускает процесс обработки заказа через <b>Routing Slip (Orchestrator)</b>.
    /// </summary>
    /// <param name="req">Данные заказа, включая ID, товар, количество и сумму.</param>
    /// <returns>HTTP 202 (Accepted), если оркестрация успешно запущена.</returns>
    /// <response code="202">Routing Slip успешно выполнен.</response>
    /// <response code="400">Ошибка валидации данных заказа.</response>
    [HttpPost("orchestrator")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces("application/json")]
    public async Task<IActionResult> Orchestrator([FromBody] CreateOrderRequest req)
    {
        var builder = new RoutingSlipBuilder(req.OrderId);

        var invArgs = new ReserveInventoryArguments(req.OrderId, req.ProductId, req.Quantity);
        var paymentArgs = new ChargePaymentArguments(req.OrderId, req.BuyerId, req.Amount);

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

        builder.AddSubscription(
            new Uri("queue:routing-slip-created"),
            RoutingSlipEvents.Completed | RoutingSlipEvents.Faulted,
            x => Task.CompletedTask
        );

        var routingSlip = builder.Build();
        await _bus.Execute(routingSlip);

        return Accepted(new { message = "Routing Slip started", orderId = req.OrderId });
    }
}