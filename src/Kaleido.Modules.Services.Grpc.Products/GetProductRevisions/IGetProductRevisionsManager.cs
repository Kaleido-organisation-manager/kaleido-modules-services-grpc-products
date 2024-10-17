using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;

public interface IGetProductRevisionsManager
{
    Task<IEnumerable<ProductRevision>> GetAllAsync(string key, CancellationToken cancellationToken = default);
}
