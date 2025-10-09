using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Domain.Repository.Abstractions;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repository.Implementations;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, string dbConnectionString)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(dbConnectionString)
        );

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
    }
}