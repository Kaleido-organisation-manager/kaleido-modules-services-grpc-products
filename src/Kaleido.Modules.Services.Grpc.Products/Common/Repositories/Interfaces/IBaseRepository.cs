using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

public interface IBaseRepository<T>
    where T : BaseEntity
{
    Task<T?> GetActiveAsync(Guid key, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> UpdateStatusAsync(Guid key, EntityStatus status, CancellationToken cancellationToken = default);
    Task<T?> DeleteAsync(Guid key, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken = default);
    Task<T?> GetRevisionAsync(Guid key, int revision, CancellationToken cancellationToken = default);
}