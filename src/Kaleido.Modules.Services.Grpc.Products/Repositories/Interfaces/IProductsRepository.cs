using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;

public interface IProductsRepository : IBaseRepository<ProductEntity>
{
    Task<IEnumerable<ProductEntity>> GetAllByCategoryIdAsync(Guid categoryKey, CancellationToken cancellationToken = default);
}