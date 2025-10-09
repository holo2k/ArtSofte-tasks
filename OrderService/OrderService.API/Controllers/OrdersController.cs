using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Dtos;
using OrderService.Application.Services.Abstractions;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    /// <summary>
    ///     Получить заказ по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор заказа.</param>
    /// <returns>Order или 404.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var order = await _service.GetOrder(id);
        return order == null ? NotFound() : Ok(order);
    }

    /// <summary>
    ///     Создать новый заказ.
    /// </summary>
    /// <param name="dto">Данные для создания заказа.</param>
    /// <returns>201 Created с Location.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var id = await _service.CreateOrder(dto.BuyerId, dto.SellerId,
            dto.Items.Select(i => (i.ProductId, i.Quantity, i.Price)).ToList());
        return CreatedAtAction(nameof(Get), new { id }, null);
    }

    /// <summary>
    ///     Обновить статус заказа.
    /// </summary>
    /// <param name="id">Идентификатор заказа.</param>
    /// <param name="dto">Новый статус заказа.</param>
    /// <returns>204 NoContent.</returns>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto dto)
    {
        await _service.UpdateStatus(id, dto.Status);
        return NoContent();
    }

    /// <summary>
    ///     Обновить список позиций заказа (только до оплаты/доставки).
    /// </summary>
    /// <param name="id">Идентификатор заказа.</param>
    /// <param name="dto">Новый набор позиций.</param>
    /// <returns>204 NoContent или 404.</returns>
    [HttpPut("{id}/items")]
    public async Task<IActionResult> UpdateItems(Guid id, [FromBody] UpdateItemsDto dto)
    {
        var order = await _service.GetOrder(id);
        if (order == null) return NotFound();
        await _service.UpdateOrderItemsAsync(id, dto.Items);
        return NoContent();
    }
}