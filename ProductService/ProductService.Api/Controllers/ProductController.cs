using Microsoft.AspNetCore.Mvc;
using ProductService.DAL.Dtos.Requests;
using ProductService.DAL.Requests;
using ProductService.Logic.Services.Abstractions;

namespace ArtSofte_project.Controllers;

[Route("api/products")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    ///     Получить список продуктов с фильтрацией, сортировкой и пагинацией.
    /// </summary>
    /// <param name="request">Параметры фильтрации и пагинации.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Список продуктов.</returns>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsRequest request, CancellationToken ct)
    {
        var products = await _productService.GetPagedAsync(request, ct);
        return Ok(products);
    }

    /// <summary>
    ///     Получить продукт по ID.
    /// </summary>
    /// <param name="id">ID продукта.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Детали продукта.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        if (product == null) return NotFound();
        return Ok(product);
    }

    /// <summary>
    ///     Создать новый продукт.
    /// </summary>
    /// <param name="request">Данные нового продукта, включая медиа.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Созданный продукт.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var created = await _productService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }

    /// <summary>
    ///     Обновить существующий продукт.
    /// </summary>
    /// <param name="id">ID продукта.</param>
    /// <param name="request">Данные для обновления, включая медиа.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Обновленный продукт или 404, если продукт не найден.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request,
        CancellationToken ct)
    {
        var updated = await _productService.UpdateAsync(id, request, ct);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    /// <summary>
    ///     Удалить продукт по ID.
    /// </summary>
    /// <param name="id">ID продукта.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>200 true, если удалено, или 404, если продукт не найден.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        var deleted = await _productService.DeleteAsync(id, ct);
        if (!deleted) return NotFound();
        return Ok(deleted);
    }

    /// <summary>
    ///     Увеличить счетчик просмотров продукта.
    /// </summary>
    /// <param name="id">ID продукта.</param>
    /// <param name="delta">На сколько увеличить (по умолчанию 1).</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>204 NoContent.</returns>
    [HttpPost("{id}/view")]
    public async Task<IActionResult> IncrementViews(Guid id, [FromQuery] int delta = 1, CancellationToken ct = default)
    {
        await _productService.IncrementViewsAsync(id, delta, ct);
        return NoContent();
    }
}