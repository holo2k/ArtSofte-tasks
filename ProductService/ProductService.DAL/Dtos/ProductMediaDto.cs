using ProductService.DAL.Models.Enums;

namespace ProductService.DAL.Dtos;

public class ProductMediaDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = "";
    public MediaType Type { get; set; }
    public int Order { get; set; }
}