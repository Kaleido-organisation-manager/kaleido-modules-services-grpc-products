using Kaleido.Modules.Services.Grpc.Products.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Constants;
using Kaleido.Modules.Services.Grpc.Products.Models;
using Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories;

public class ProductsRepository : IProductsRepository
{
    private readonly ILogger<ProductsRepository> _logger;
    private readonly ProductsDbContext _dbContext;

    public ProductsRepository(ILogger<ProductsRepository> logger, ProductsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<ProductEntity?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetProduct called with Id: {Id}", id);
        var product = await _dbContext.Products.FindAsync(id, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product with Id: {Id} not found", id);
            return null;
        }
        return product;
    }

    public async Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProducts called");
        return await _dbContext.Products.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductEntity>> GetAllByCategoryIdAsync(string categoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProductsByCategoryId called with CategoryId: {CategoryId}", categoryId);
        return await _dbContext.Products.Where(p => p.CategoryKey == categoryId).ToListAsync(cancellationToken);
    }

    public async Task<ProductEntity> CreateAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating Product with key: {Key}", product.Key);
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product with key: {Key} created", product.Key);
        return product;
    }

    public async Task<ProductEntity> UpdateAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        if (product.Key == null)
        {
            throw new ArgumentNullException(nameof(product.Key));
        }

        _logger.LogInformation("Updating Product with key: {key}", product.Key);
        await ArchiveAsync(product.Key, cancellationToken);
        return await CreateAsync(product, cancellationToken);
    }

    public async Task<ProductEntity?> ArchiveAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Archiving Product with key: {key}", key);
        var activeProduct = await GetActiveAsync(key, cancellationToken);
        if (activeProduct == null)
        {
            _logger.LogWarning("Product with key: {key} not found", key);
            return null;
        }

        activeProduct.Status = EntityStatus.Archived;
        _dbContext.Products.Update(activeProduct);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product with key: {key} archived", key);
        return activeProduct;
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