using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Services.Abstractions;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/buyer/orders")]
public class BuyerOrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public BuyerOrdersController(IOrderService service) => _service = service;

    /// <summary>
    /// Получить список заказов покупателя.
    /// </summary>
    /// <param name="buyerId">Идентификатор покупателя.</param>
    /// <returns>Список заказов.</returns>
    [HttpGet("{buyerId}")]
    public async Task<IActionResult> GetBuyerOrders(Guid buyerId)
    {
        var orders = await _service.GetBuyerOrders(buyerId);
        return Ok(orders);
    }
}