using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;

public interface IProductPricesRepository : IBaseRepository<ProductPriceEntity>
{
    Task<IEnumerable<ProductPriceEntity>> GetAllByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductPriceEntity>> CreateRangeAsync(IEnumerable<ProductPriceEntity> productPrices, CancellationToken cancellationToken = default);
    Task DeleteByProductIdAsync(string productId, CancellationToken cancellationToken = default);
}