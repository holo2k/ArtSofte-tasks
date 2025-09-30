using System.Text.Json.Serialization;
using ProductService.DAL.Models.Enums;

namespace ProductService.DAL.Models;

public class ProductMedia
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Url { get; set; } = "";
    public MediaType Type { get; set; }
    public int Order { get; set; }

    [JsonIgnore] public Product? Product { get; set; }
}