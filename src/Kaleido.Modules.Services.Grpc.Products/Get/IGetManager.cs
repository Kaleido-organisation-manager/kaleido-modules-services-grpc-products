using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Get;
public interface IGetManager
{
    Task<Product?> GetAsync(string key, CancellationToken cancellationToken = default);
}
