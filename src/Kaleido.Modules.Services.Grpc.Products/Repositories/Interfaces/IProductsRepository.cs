using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;

public interface IProductsRepository
{
    Task<ProductEntity?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductEntity>> GetAllByCategoryIdAsync(string categoryId, CancellationToken cancellationToken = default);
    Task<ProductEntity> CreateAsync(ProductEntity product, CancellationToken cancellationToken = default);
    Task<ProductEntity> UpdateAsync(ProductEntity product, CancellationToken cancellationToken = default);
    Task<ProductEntity?> ArchiveAsync(string id, CancellationToken cancellationToken = default);
    Task<ProductEntity?> GetActiveAsync(string id, CancellationToken cancellationToken = default);
}