using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Services.Abstractions;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/v1/seller/orders")]
public class SellerOrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public SellerOrdersController(IOrderService service)
    {
        _service = service;
    }

    /// <summary>
    ///     Получить список заказов продавца.
    /// </summary>
    /// <param name="sellerId">Идентификатор продавца.</param>
    /// <returns>Список заказов.</returns>
    [HttpGet("{sellerId}")]
    public async Task<IActionResult> GetSellerOrders(Guid sellerId)
    {
        var orders = await _service.GetSellerOrders(sellerId);
        return Ok(orders);
    }
}