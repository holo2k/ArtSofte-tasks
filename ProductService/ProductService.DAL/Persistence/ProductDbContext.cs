using Microsoft.EntityFrameworkCore;
using ProductService.DAL.Models;

namespace ProductService.DAL.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> opts) : base(opts)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductMedia> ProductMedias => Set<ProductMedia>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Favorite> Favorites => Set<Favorite>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Product>().HasKey(p => p.Id);
        model.Entity<ProductMedia>().HasKey(m => m.Id);
        model.Entity<Review>().HasKey(r => r.Id);
        model.Entity<Favorite>().HasKey(f => f.Id);

        model.Entity<Product>()
            .HasMany(p => p.Media)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        model.Entity<Product>()
            .HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        model.Entity<Favorite>()
            .HasIndex(f => new { f.UserId, f.ProductId })
            .IsUnique();
    }
}