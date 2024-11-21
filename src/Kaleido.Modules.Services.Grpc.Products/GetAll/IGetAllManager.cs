using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public interface IGetAllManager
{
    Task<IEnumerable<ManagerResponse>> GetAllProductsAsync(CancellationToken cancellationToken);
}
