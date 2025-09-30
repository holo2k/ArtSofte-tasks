using ProductService.DAL.Dtos;
using ProductService.DAL.Models.Enums;

namespace ProductService.DAL.Requests;

public class UpdateProductRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Category Category { get; set; }
    public int Views { get; set; }
    public decimal AvgRating { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    ///     Медиа файлы продукта (только URL, тип и порядок)
    /// </summary>
    public IEnumerable<ProductMediaDto>? Media { get; set; }
}