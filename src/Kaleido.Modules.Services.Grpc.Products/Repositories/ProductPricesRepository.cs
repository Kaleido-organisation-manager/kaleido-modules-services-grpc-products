using Kaleido.Modules.Services.Grpc.Products.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Constants;
using Kaleido.Modules.Services.Grpc.Products.Models;
using Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories;

public class ProductPricesRepository : BaseRepository<ProductPriceEntity, ProductPricesDbContext>, IProductPricesRepository
{
    public ProductPricesRepository(ILogger<ProductPricesRepository> logger, ProductPricesDbContext dbContext)
    : base(logger, dbContext, dbContext.ProductPrices)
    {
    }

    public async Task<IEnumerable<ProductPriceEntity>> GetAllByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all product prices by product id: {ProductId}", productId);

        var productPrices = await _dbContext.ProductPrices
            .Where(productPrice => productPrice.ProductKey == productId)
            .Where(productPrice => productPrice.Status == EntityStatus.Active)
            .ToListAsync(cancellationToken);

        return productPrices;
    }

    public async Task<IEnumerable<ProductPriceEntity>> CreateRangeAsync(IEnumerable<ProductPriceEntity> productPrices, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating product prices");

        await _dbContext.ProductPrices.AddRangeAsync(productPrices, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return productPrices;
    }

    public async Task DeleteByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product prices by product id: {ProductId}", productId);

        // resolve product prices by product id
        var productPrices = await GetAllByProductIdAsync(productId, cancellationToken);

        // update all price states to deleted
        foreach (var productPrice in productPrices)
        {
            await DeleteAsync(productPrice.Key!, cancellationToken);
        }
    }
}