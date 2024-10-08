using Kaleido.Modules.Services.Grpc.Products.Constants;
using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;

public interface IBaseRepository<T>
    where T : BaseEntity
{
    Task<T> GetAsync(Guid key, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> UpdateStatusAsync(Guid key, EntityStatus status, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid key, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetRevisionsAsync(Guid key, CancellationToken cancellationToken = default);
    Task<T> GetRevisionAsync(Guid key, int revision, CancellationToken cancellationToken = default);
}