using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Models;

public class ProductWithPrices : ProductEntity
{
    public required IEnumerable<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> Prices { get; set; }
}