using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public interface IGetManager
{
    Task<ManagerResponse> GetAsync(Guid key, CancellationToken cancellationToken = default);
}