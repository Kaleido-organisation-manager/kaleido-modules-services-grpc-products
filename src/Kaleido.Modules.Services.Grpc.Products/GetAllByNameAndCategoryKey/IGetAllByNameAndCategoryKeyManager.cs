using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByNameAndCategoryKey;

public interface IGetAllByNameAndCategoryKeyManager
{
    Task<IEnumerable<Product>> GetAllByNameAndCategoryKeyAsync(string name, string categoryKey, CancellationToken cancellationToken = default);
}