using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Models;

public readonly struct ManagerResponse
{
    public readonly EntityLifeCycleResult<ProductEntity, ProductRevisionEntity> Product;
    public readonly IEnumerable<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> ProductPrices;

    public ManagerResponse(EntityLifeCycleResult<ProductEntity, ProductRevisionEntity> product, IEnumerable<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> productPrices)
    {
        Product = product;
        ProductPrices = productPrices;
    }
}