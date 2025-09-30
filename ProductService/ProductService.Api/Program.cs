using System.Reflection;
using Core.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductService.DAL;
using ProductService.DAL.Persistence;
using ProductService.Logic.AutoMapper;
using ProductService.Logic.Services.Abstractions;
using ProductService.Logic.Services.Implementations;

namespace ArtSofte_project;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddSwagger();

        builder.Services.AddPostgresDal(builder.Configuration.GetConnectionString("Default")!);

        builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

        builder.Services.AddApplicationServices();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API V1"); });

        app.UseHttpsRedirection();
        app.MapControllers();
        await app.RunAsync();
    }
}

public static class ServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService.Logic.Services.Implementations.ProductService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IReviewService, ReviewService>();

        services.AddSingleton<IEventPublisher, RabbitMqPublisher>();
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Product API",
                Version = "v1",
                Description = "API for working with products"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath, true);
        });
    }
}