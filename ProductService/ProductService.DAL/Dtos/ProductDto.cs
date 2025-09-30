using ProductService.DAL.Models.Enums;

namespace ProductService.DAL.Dtos;

public class ProductDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Category Category { get; set; }
    public int Views { get; set; }
    public decimal AvgRating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public IEnumerable<ProductMediaDto> Media { get; set; } = Array.Empty<ProductMediaDto>();
    public IEnumerable<ReviewDto> Reviews { get; set; } = Array.Empty<ReviewDto>();
}