using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;

public interface IProductPricesRepository
{
    Task<IEnumerable<ProductPriceEntity>> GetAllByProductIdAsync(string productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductPriceEntity>> CreateRangeAsync(IEnumerable<ProductPriceEntity> productPrices, CancellationToken cancellationToken = default);
}