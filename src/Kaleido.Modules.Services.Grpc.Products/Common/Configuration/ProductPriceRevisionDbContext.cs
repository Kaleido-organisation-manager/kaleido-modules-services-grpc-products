using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Configuration;

public class ProductPriceRevisionDbContext : DbContext, IKaleidoDbContext<ProductPriceRevisionEntity>
{
    public DbSet<ProductPriceRevisionEntity> Items { get; set; } = null!;

    public ProductPriceRevisionDbContext(DbContextOptions<ProductPriceRevisionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductPriceRevisionEntity>(entity =>
        {
            DefaultOnModelCreatingMethod.ForBaseRevisionEntity(entity);
            entity.ToTable("ProductPriceRevisions");
        });
    }
}
