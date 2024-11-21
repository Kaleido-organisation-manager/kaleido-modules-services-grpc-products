using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Configuration;

public class ProductEntityDbContext : DbContext, IKaleidoDbContext<ProductEntity>
{
    public DbSet<ProductEntity> Items { get; set; } = null!;

    public ProductEntityDbContext(DbContextOptions<ProductEntityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products");

            entity.Property(x => x.Name).IsRequired().HasColumnType("varchar(100)");
            entity.Property(x => x.Description).HasColumnType("varchar(255)");
            entity.Property(x => x.ImageUrl).HasColumnType("varchar(255)");
            entity.Property(x => x.CategoryKey).IsRequired().HasColumnType("varchar(36)");

            DefaultOnModelCreatingMethod.ForBaseEntity(entity);
        });
    }
}
