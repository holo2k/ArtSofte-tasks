using System.Reflection;
using Core.EventBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderService.Application;
using OrderService.Application.EventHandlers;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Default");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orders API",
        Version = "v1",
        Description = "API for working with orders"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, true);
});

builder.Services.AddControllers();

builder.Services.AddSingleton<IEventSubscriber, RabbitMqSubscriber>();
builder.Services.AddInfrastructure(conn!);
builder.Services.AddApplication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await dbContext.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var subscriber = scope.ServiceProvider.GetRequiredService<IEventSubscriber>();
    var handler = scope.ServiceProvider.GetRequiredService<ProductEventHandler>();

    await subscriber.SubscribeAsync(
        "order.product-sync",
        "product.*",
        handler.HandleAsync
    );
}

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1"); });

app.UseHttpsRedirection();

app.MapControllers();

app.Run();