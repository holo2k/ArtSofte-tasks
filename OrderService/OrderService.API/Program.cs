using System.Reflection;
using Core.EventBus;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderService.API.Activities;
using OrderService.API.Consumers;
using OrderService.Application;
using OrderService.Application.Activities;
using OrderService.Application.EventHandlers;
using OrderService.Application.Sagas;
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
        Description = "API for working with orders (Saga + Orchestration)"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, true);
});

builder.Services.AddControllers();

builder.Services.AddDbContext<SagaDbContext>(options =>
{
    options.UseNpgsql(conn, npg => npg.MigrationsAssembly("OrderService.Infrastructure"));
});

var rabbitSection = builder.Configuration.GetSection("RabbitMq");
var rabbitUri = rabbitSection.GetValue<string>("Uri");
if (string.IsNullOrWhiteSpace(rabbitUri))
{
    var host = rabbitSection.GetValue<string>("Host", "rabbit-mq");
    var port = rabbitSection.GetValue("Port", 5672);
    var user = rabbitSection.GetValue<string>("User", "guest");
    var pass = rabbitSection.GetValue<string>("Pass", "guest");
    rabbitUri = $"rabbitmq://{Uri.EscapeDataString(user)}:{Uri.EscapeDataString(pass)}@{host}:{port}/";
}

builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMqPublisher>>();
    return new RabbitMqPublisher(
        logger,
        rabbitSection.GetValue<string>("Host", "rabbit-mq"),
        rabbitSection.GetValue("Port", 5672),
        rabbitSection.GetValue<string>("User", "guest"),
        rabbitSection.GetValue<string>("Pass", "guest"),
        rabbitSection.GetValue<string>("ExchangeName", "product.events")
    );
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RoutingSlipCreatedHandler>();
    x.AddConsumer<ReserveInventoryConsumer>();
    x.AddConsumer<ChargePaymentConsumer>();
    x.AddConsumer<CreateOrderConsumer>();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.AddDbContext<DbContext, SagaDbContext>((provider, options) =>
            {
                options.UseNpgsql(conn, npg => npg.MigrationsAssembly("OrderService.Infrastructure"));
            });
        });

    x.AddExecuteActivity<ReserveInventoryActivity, ReserveInventoryArguments>();
    x.AddExecuteActivity<ChargePaymentActivity, ChargePaymentArguments>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitUri);

        cfg.ReceiveEndpoint("reserve-inventory_execute",
            e => e.ExecuteActivityHost<ReserveInventoryActivity, ReserveInventoryArguments>(context));

        cfg.ReceiveEndpoint("charge-payment_execute",
            e => e.ExecuteActivityHost<ChargePaymentActivity, ChargePaymentArguments>(context));

        cfg.ReceiveEndpoint("create-order_queue", e => { e.ConfigureConsumer<CreateOrderConsumer>(context); });

        cfg.ReceiveEndpoint("routing-slip-created", e => { e.ConfigureConsumer<RoutingSlipCreatedHandler>(context); });

        cfg.ConfigureEndpoints(context);
    });
});

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
    var sagaDb = scope.ServiceProvider.GetRequiredService<SagaDbContext>();
    await sagaDb.Database.MigrateAsync();
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