using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;

public interface IGetAllByCategoryKeyManager
{
    Task<IEnumerable<Product>> GetAllAsync(string categoryKey, CancellationToken cancellationToken = default);
}