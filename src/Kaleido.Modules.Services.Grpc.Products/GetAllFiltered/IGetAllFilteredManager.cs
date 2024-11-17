using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllFiltered;

public interface IGetAllFilteredManager
{
    Task<IEnumerable<ManagerResponse>> GetAllFilteredAsync(
        string? name,
        string? categoryKey,
        CancellationToken cancellationToken);
}