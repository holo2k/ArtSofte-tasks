using ProductService.DAL.Models.Enums;

namespace ProductService.DAL.Dtos.Requests;

public class GetProductsRequest
{
    public string? Search { get; set; }
    public Category? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Sort { get; set; } // price, rating, newest
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}