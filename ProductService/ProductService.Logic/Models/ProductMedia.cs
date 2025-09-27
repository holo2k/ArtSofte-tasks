using ProductService.Logic.Models.Enums;

namespace ProductService.Logic.Models;

public class ProductMedia
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = "";
    public MediaType Type { get; set; }
    public int Order { get; set; }
}