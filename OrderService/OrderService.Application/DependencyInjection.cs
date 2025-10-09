using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.EventHandlers;
using OrderService.Application.Services.Abstractions;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductEventHandler>();
        services.AddScoped<IOrderService, Services.Implementations.OrderService>();
    }
}