using ProductService.DAL.Models.Enums;

namespace ProductService.DAL.Dtos.Requests;

public class ProductMediaCreateRequest
{
    public string Url { get; init; } = "";
    public MediaType Type { get; init; }
    public int Order { get; init; }
}