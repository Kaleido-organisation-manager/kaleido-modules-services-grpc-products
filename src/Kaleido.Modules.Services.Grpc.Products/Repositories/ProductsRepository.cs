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

    public async Task<IEnumerable<ProductEntity>> GetAllByCategoryIdAsync(Guid categoryKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProductsByCategoryId called with CategoryId: {CategoryId}", categoryKey);
        return await _dbContext.Products
            .Where(p => p.CategoryKey == categoryKey)
            .Where(p => p.Status == EntityStatus.Active)
            .ToListAsync(cancellationToken);
    }

}