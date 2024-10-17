using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevision;

public interface IGetProductRevisionManager
{
    Task<ProductRevision?> GetAsync(string key, int revision, CancellationToken cancellationToken = default);
}
