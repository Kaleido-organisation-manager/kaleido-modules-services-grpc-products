using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

public interface IProductRepository : IBaseRepository<ProductEntity>
{
    Task<IEnumerable<ProductEntity>> GetAllByCategoryIdAsync(Guid categoryKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductEntity>> GetAllByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductEntity>> GetAllByNameAndCategoryKeyAsync(string name, Guid categoryKey, CancellationToken cancellationToken = default);
}