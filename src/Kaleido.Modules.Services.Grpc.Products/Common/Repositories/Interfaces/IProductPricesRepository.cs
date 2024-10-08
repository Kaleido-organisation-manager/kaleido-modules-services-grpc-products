using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

public interface IProductPricesRepository : IBaseRepository<ProductPriceEntity>
{
    Task<IEnumerable<ProductPriceEntity>> GetAllByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductPriceEntity>> CreateRangeAsync(IEnumerable<ProductPriceEntity> productPrices, CancellationToken cancellationToken = default);
    Task DeleteByProductKeyAsync(Guid productKey, CancellationToken cancellationToken = default);
    Task<ProductPriceEntity> GetRevisionAsync(Guid productKey, Guid currencyKey, int revision, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductPriceEntity>> GetAllRevisionsAsync(Guid productKey, Guid currencyKey, CancellationToken cancellationToken = default);
}
