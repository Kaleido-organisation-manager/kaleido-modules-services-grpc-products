using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public interface IDeleteManager
{
    Task<ProductEntity?> DeleteAsync(string key, CancellationToken cancellationToken = default);
}