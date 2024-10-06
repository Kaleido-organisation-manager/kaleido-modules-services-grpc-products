using Kaleido.Modules.Services.Grpc.Products.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Configuration;

public class ProductPricesDbContext : DbContext
{
    public ProductPricesDbContext(DbContextOptions<ProductPricesDbContext> options) : base(options)
    {
    }

    public DbSet<ProductPriceEntity> ProductPrices { get; set; }
}
