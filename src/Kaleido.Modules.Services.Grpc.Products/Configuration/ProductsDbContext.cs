using Kaleido.Modules.Services.Grpc.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Configuration;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options)
    {
    }

    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ProductPriceEntity> ProductPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Products");
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("uuid");
            entity.Property(e => e.Key).IsRequired().HasColumnType("varchar(36)");
            entity.Property(e => e.Name).IsRequired().HasColumnType("varchar(255)");
            entity.Property(e => e.CategoryKey).IsRequired().HasColumnType("uuid");
            entity.Property(e => e.Status).IsRequired().HasColumnType("varchar(10)");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.ImageUrl).HasColumnType("text");
            entity.Property(e => e.Revision).IsRequired().HasColumnType("int");

            entity.HasIndex(e => e.CategoryKey);
            entity.HasIndex(e => e.Key);


        });

        modelBuilder.Entity<ProductPriceEntity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ProductPrices");
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnType("uuid");
            entity.Property(e => e.Key).IsRequired().HasColumnType("varchar(36)");
            entity.Property(e => e.ProductKey).IsRequired().HasColumnType("varchar(36)");
            entity.Property(e => e.Price).IsRequired().HasColumnType("float");
            entity.Property(e => e.CurrencyKey).IsRequired().HasColumnType("uuid");
            entity.Property(e => e.Status).IsRequired().HasColumnType("varchar(10)");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
            entity.Property(e => e.Revision).IsRequired().HasColumnType("int");

            entity.HasIndex(e => e.ProductKey);
            entity.HasIndex(e => e.Key);
        });
    }
}