using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public interface IUpdateManager
{
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);
}