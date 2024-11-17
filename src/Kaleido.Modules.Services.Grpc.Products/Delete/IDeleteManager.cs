using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public interface IDeleteManager
{
    Task<ManagerResponse> DeleteAsync(Guid key, CancellationToken cancellationToken = default);
}
