

using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllRevisions;

public interface IGetAllRevisionsManager
{
    Task<IEnumerable<ManagerResponse>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken = default);
}