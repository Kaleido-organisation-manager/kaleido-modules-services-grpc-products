using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Repositories;

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