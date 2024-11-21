using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.GetRevision;

public interface IGetRevisionManager
{
    Task<ManagerResponse> GetRevisionAsync(Guid key, DateTime createdAt, CancellationToken cancellationToken = default);
}
