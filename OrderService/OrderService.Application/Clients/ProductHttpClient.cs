using Core.HttpLogic.Services;
using Core.HttpLogic.Services.Interfaces;
using OrderService.Application.Dtos;

namespace OrderService.Application.Clients;

public class ProductHttpClient : IProductClient
{
    private readonly HttpConnectionData _conn;
    private readonly IHttpRequestService _http;

    public ProductHttpClient(IHttpRequestService http)
    {
        _http = http;
        _conn = new HttpConnectionData("product-api");
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken ct = default)
    {
        var req = new HttpRequestData
        {
            Method = HttpMethod.Get,
            Uri = new Uri($"http://productservice-api/api/products/{id}")
        };

        var resp = await _http.SendRequestAsync<ProductDto>(req, _conn with { CancellationToken = ct });
        return resp.IsSuccessStatusCode ? resp.Body : null;
    }
}