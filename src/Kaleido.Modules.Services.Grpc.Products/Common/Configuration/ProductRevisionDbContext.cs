using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Configuration;

public class ProductRevisionDbContext : DbContext, IKaleidoDbContext<ProductRevisionEntity>
{
    public DbSet<ProductRevisionEntity> Items { get; set; } = null!;

    public ProductRevisionDbContext(DbContextOptions<ProductRevisionDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductRevisionEntity>(entity =>
        {
            DefaultOnModelCreatingMethod.ForBaseRevisionEntity(entity);
            entity.ToTable("ProductRevisions");
        });
    }
}
