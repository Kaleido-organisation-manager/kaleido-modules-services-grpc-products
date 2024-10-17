using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public interface ICreateManager
{
    Task<Product> CreateAsync(CreateProduct product, CancellationToken cancellationToken = default);
}
