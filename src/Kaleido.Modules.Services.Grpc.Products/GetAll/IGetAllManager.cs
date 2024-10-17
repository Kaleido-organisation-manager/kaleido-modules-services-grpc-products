using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public interface IGetAllManager
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
}