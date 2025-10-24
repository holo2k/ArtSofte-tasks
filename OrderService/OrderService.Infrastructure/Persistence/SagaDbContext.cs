using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Sagas;

namespace OrderService.Infrastructure.Persistence;

public class SagaDbContext : MassTransit.EntityFrameworkCoreIntegration.SagaDbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options, IEnumerable<ISagaClassMap> configurations) :
        base(options)
    {
        Configurations = configurations;
    }

    public DbSet<OrderState> OrderStates { get; set; } = null!;

    protected override IEnumerable<ISagaClassMap> Configurations { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddTransactionalOutboxEntities();

        modelBuilder.Entity<OrderState>(entity =>
        {
            entity.ToTable("OrderState");
            entity.HasKey(x => x.CorrelationId);
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.HasIndex(x => x.CurrentState).HasDatabaseName("IX_OrderState_CurrentState");
            entity.HasIndex(x => x.OrderId).HasDatabaseName("IX_OrderState_OrderId");
            entity.Property<DateTime?>("UpdatedAt").IsRequired(false);
        });

        base.OnModelCreating(modelBuilder);
    }
}