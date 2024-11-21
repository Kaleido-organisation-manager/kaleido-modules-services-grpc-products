using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public interface ICreateManager
{
    Task<ManagerResponse> CreateAsync(ProductEntity productEntity, IEnumerable<ProductPrice> productPrices, CancellationToken cancellationToken = default);
}