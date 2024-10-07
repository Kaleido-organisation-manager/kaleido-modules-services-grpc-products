using Kaleido.Modules.Services.Grpc.Products.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Constants;
using Kaleido.Modules.Services.Grpc.Products.Models;
using Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories;

public class ProductsRepository : BaseRepository<ProductEntity, ProductsDbContext>, IProductsRepository
{
    public ProductsRepository(ILogger<ProductsRepository> logger, ProductsDbContext dbContext)
    : base(logger, dbContext, dbContext.Products)
    {
    }

    public async Task<IEnumerable<ProductEntity>> GetAllByCategoryIdAsync(string categoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProductsByCategoryId called with CategoryId: {CategoryId}", categoryId);
        return await _dbContext.Products
            .Where(p => p.CategoryKey == categoryId)
            .Where(p => p.Status == EntityStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductEntity?> GetActiveAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting active product with key: {Key}", key);
        var product = await _dbContext.Products.Where(p => p.Key == key && p.Status == EntityStatus.Active).FirstOrDefaultAsync(cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product with key: {Id} not found", key);
            return null;
        }
        return product;
    }

}