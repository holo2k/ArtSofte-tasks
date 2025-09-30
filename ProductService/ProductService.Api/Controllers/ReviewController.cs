using Microsoft.AspNetCore.Mvc;
using ProductService.DAL.Dtos.Requests;
using ProductService.Logic.Services.Abstractions;

namespace ArtSofte_project.Controllers;

[ApiController]
[Route("api/review")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    ///     Добавить новый отзыв к продукту.
    /// </summary>
    /// <param name="productId">ID продукта.</param>
    /// <param name="request">Данные отзыва.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Созданный отзыв.</returns>
    [HttpPost("{productId}")]
    public async Task<IActionResult> Create(Guid productId, [FromBody] CreateReviewRequest request,
        CancellationToken ct)
    {
        try
        {
            var created = await _reviewService.AddReviewAsync(productId, request, ct);
            return CreatedAtAction(nameof(GetByProduct), new { productId }, created);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Product with ID {productId} not found.");
        }
    }

    /// <summary>
    ///     Ответить на отзыв (только продавец может ответить).
    /// </summary>
    /// <param name="reviewId">ID отзыва.</param>
    /// <param name="text">Текст ответа.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Обновлённый отзыв с ответом.</returns>
    [HttpPost("{reviewId}/reply")]
    public async Task<IActionResult> Reply(Guid reviewId, [FromBody] string text,
        CancellationToken ct)
    {
        try
        {
            var replied = await _reviewService.ReplyAsync(reviewId, text, ct);
            if (replied == null) return NotFound($"Review with ID {reviewId} not found.");
            return Ok(replied);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    ///     Получить список отзывов для продукта с пагинацией.
    /// </summary>
    /// <param name="productId">ID продукта.</param>
    /// <param name="page">Номер страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>Список отзывов.</returns>
    [HttpGet("{productId}")]
    public async Task<IActionResult> GetByProduct(Guid productId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var reviews = await _reviewService.GetByProductAsync(productId, page, pageSize, ct);
        return Ok(reviews);
    }

    /// <summary>
    ///     Удалить отзыв.
    /// </summary>
    /// <param name="reviewId">ID отзыва.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>204 NoContent.</returns>
    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> Delete(Guid reviewId, CancellationToken ct)
    {
        await _reviewService.DeleteAsync(reviewId, ct);
        return NoContent();
    }
}