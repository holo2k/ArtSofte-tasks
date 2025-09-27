using ProductService.Logic.Models.Enums;

namespace ProductService.Logic.Models;

public class Product
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
}