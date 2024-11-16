using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Configuration;

public class ProductPriceEntityDbContext : DbContext, IKaleidoDbContext<ProductPriceEntity>
{
    public DbSet<ProductPriceEntity> Items { get; set; } = null!;

    public ProductPriceEntityDbContext(DbContextOptions<ProductPriceEntityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductPriceEntity>(entity =>
        {
            entity.ToTable("ProductPrices");

            entity.Property(x => x.ProductKey).IsRequired().HasColumnType("varchar(36)");
            entity.Property(x => x.CurrencyKey).IsRequired().HasColumnType("varchar(36)");
            entity.Property(x => x.Value).IsRequired().HasColumnType("decimal(10, 2)");

            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
        });
    }
}
