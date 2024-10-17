using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByName;

public interface IGetAllByNameManager
{
    Task<IEnumerable<Product>> GetAllByNameAsync(string name, CancellationToken cancellationToken = default);
}
