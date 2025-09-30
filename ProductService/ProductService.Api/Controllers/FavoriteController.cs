using Microsoft.AspNetCore.Mvc;
using ProductService.Logic.Services.Abstractions;

namespace ArtSofte_project.Controllers;

[ApiController]
[Route("api/favorite")]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoriteController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>
    ///     Добавить продукт в избранное.
    /// </summary>
    /// <param name="productId">ID продукта.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>200 Favorite model</returns>
    [HttpPost("{productId}")]
    public async Task<IActionResult> AddToFavorite(Guid productId, CancellationToken ct)
    {
        return Ok(await _favoriteService.AddFavoriteAsync(productId, ct));
    }

    /// <summary>
    ///     Убрать продукт из избранного.
    /// </summary>
    /// <param name="productId">ID продукта.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    /// <returns>200 true if success</returns>
    [HttpDelete("{productId}/favorite")]
    public async Task<IActionResult> DeleteFromFavorite(Guid productId, CancellationToken ct)
    {
        return Ok(await _favoriteService.RemoveFavoriteAsync(productId, ct));
    }
}