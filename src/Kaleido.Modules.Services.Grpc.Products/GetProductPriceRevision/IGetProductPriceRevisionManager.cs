using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevision;

public interface IGetProductPriceRevisionManager
{
    Task<ProductPriceRevision> GetAsync(string key, string currency, int revision, CancellationToken cancellationToken = default);
}
