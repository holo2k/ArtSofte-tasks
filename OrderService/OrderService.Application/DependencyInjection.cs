using Core.HttpLogic.Services;
using Core.HttpLogic.Services.Interfaces;
using Core.TraceIdLogic;
using Core.TraceLogic.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderService.Application.Clients;
using OrderService.Application.EventHandlers;
using OrderService.Application.Services.Abstractions;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductEventHandler>();
        services.AddScoped<IOrderService, Services.Implementations.OrderService>();

        services.AddHttpClient();
        services.AddTransient<IHttpConnectionService, HttpConnectionService>();
        services.TryAddTransient<IHttpRequestService, HttpRequestService>();

        services.AddScoped<TraceIdAccessor>();
        services.TryAddScoped<ITraceWriter>(p => p.GetRequiredService<TraceIdAccessor>());
        services.TryAddScoped<ITraceReader>(p => p.GetRequiredService<TraceIdAccessor>());
        services.TryAddScoped<ITraceIdAccessor>(p => p.GetRequiredService<TraceIdAccessor>());

        services.AddScoped<IProductClient, ProductHttpClient>();
    }
}