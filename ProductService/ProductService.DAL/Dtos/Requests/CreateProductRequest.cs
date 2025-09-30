using ProductService.DAL.Models.Enums;

namespace ProductService.DAL.Dtos.Requests;

public class CreateProductRequest
{
    public string Title { get; init; } = "No Title";
    public string Description { get; init; } = "";
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public Category Category { get; init; }

    /// <summary>
    ///     Медиа файлы продукта (только URL, тип и порядок отображения)
    /// </summary>
    public IEnumerable<ProductMediaDto>? Media { get; set; }
}