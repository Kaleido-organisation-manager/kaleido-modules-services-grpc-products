using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.EntityFrameworkCore;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Repositories;

public class ProductPricesRepository : BaseRepository<ProductPriceEntity, ProductsDbContext>, IProductPricesRepository
{
    public ProductPricesRepository(ILogger<ProductPricesRepository> logger, ProductsDbContext dbContext)
    : base(logger, dbContext, dbContext.ProductPrices)
    {
    }

    public async Task<IEnumerable<ProductPriceEntity>> GetAllByProductKeyAsync(Guid productId, CancellationToken cancellationToken = default)
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

    public async Task DeleteByProductKeyAsync(Guid productKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product prices by product id: {ProductId}", productKey);

        // resolve product prices by product id
        var productPrices = await GetAllByProductKeyAsync(productKey, cancellationToken);

        // update all price states to deleted
        foreach (var productPrice in productPrices)
        {
            await DeleteAsync(productPrice.Key!, cancellationToken);
        }
    }

    public async Task<ProductPriceEntity?> GetRevisionAsync(Guid productKey, Guid currencyKey, int revision, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product price revision for product id: {ProductId} and currency id: {CurrencyId} and revision: {Revision}", productKey, currencyKey, revision);

        var productPrice = await _dbContext.ProductPrices
            .Where(productPrice => productPrice.ProductKey == productKey)
            .Where(productPrice => productPrice.CurrencyKey == currencyKey)
            .Where(productPrice => productPrice.Revision == revision)
            .FirstOrDefaultAsync(cancellationToken);

        return productPrice;
    }

    public async Task<IEnumerable<ProductPriceEntity>> GetAllRevisionsAsync(Guid productKey, Guid currencyKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all product price revisions for product id: {ProductId} and currency id: {CurrencyId}", productKey, currencyKey);

        var productPrices = await _dbContext.ProductPrices
            .Where(productPrice => productPrice.ProductKey == productKey)
            .Where(productPrice => productPrice.CurrencyKey == currencyKey)
            .ToListAsync(cancellationToken);

        return productPrices;
    }
}
