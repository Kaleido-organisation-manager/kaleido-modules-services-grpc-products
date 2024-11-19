using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public interface IUpdateManager
{
    Task<ManagerResponse> UpdateAsync(
        Guid key,
        ProductEntity product,
        IEnumerable<ProductPriceEntity> prices,
        CancellationToken cancellationToken = default);
}