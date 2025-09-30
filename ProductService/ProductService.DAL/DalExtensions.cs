using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductService.DAL.Persistence;
using ProductService.DAL.Repository.Abstractions;
using ProductService.DAL.Repository.Implementations;

namespace ProductService.DAL;

public static class DalExtensions
{
    public static IServiceCollection AddPostgresDal(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ProductDbContext>(opt =>
            opt.UseNpgsql(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        return services;
    }
}