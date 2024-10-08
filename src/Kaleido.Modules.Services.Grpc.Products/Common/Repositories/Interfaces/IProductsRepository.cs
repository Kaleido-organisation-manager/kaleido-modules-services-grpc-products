using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

public interface IProductsRepository : IBaseRepository<ProductEntity>
{
    Task<IEnumerable<ProductEntity>> GetAllByCategoryIdAsync(Guid categoryKey, CancellationToken cancellationToken = default);
}