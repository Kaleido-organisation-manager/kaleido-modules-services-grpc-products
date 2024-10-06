using Kaleido.Modules.Services.Grpc.Products.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Models;
using Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories;

public class ProductPricesRepository : IProductPricesRepository
{
    private readonly ILogger<ProductPricesRepository> _logger;
    private readonly ProductPricesDbContext _dbContext;

    public ProductPricesRepository(ILogger<ProductPricesRepository> logger, ProductPricesDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ProductPriceEntity>> GetAllByProductIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all product prices by product id: {ProductId}", productId);

        var productPrices = await _dbContext.ProductPrices
            .Where(productPrice => productPrice.ProductKey == productId)
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
}