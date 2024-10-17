using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Repositories;

public class ProductRepository : BaseRepository<ProductEntity, ProductsDbContext>, IProductRepository
{
    public ProductRepository(ILogger<ProductRepository> logger, ProductsDbContext dbContext)
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

    public async Task<IEnumerable<ProductEntity>> GetAllByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProductsByName called with Name: {Name}", name);
        return await _dbContext.Products
            .Where(p => p.Name.ToLower().Contains(name.ToLower()))
            .Where(p => p.Status == EntityStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductEntity>> GetAllByNameAndCategoryKeyAsync(string name, Guid categoryKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProductsByNameAndCategoryKey called with Name: {Name} and CategoryKey: {CategoryKey}", name, categoryKey);
        return await _dbContext.Products
            .Where(p => p.Name.ToLower().Contains(name.ToLower()))
            .Where(p => p.CategoryKey == categoryKey)
            .Where(p => p.Status == EntityStatus.Active)
            .ToListAsync(cancellationToken);
    }

}