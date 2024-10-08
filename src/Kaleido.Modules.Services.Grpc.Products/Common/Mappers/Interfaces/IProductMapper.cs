using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;

public interface IProductMapper
{
    Product FromEntities(ProductEntity productEntity, IEnumerable<ProductPriceEntity> productPriceEntities);
    ProductEntity ToCreateEntity(Product product, int revision = 1);
    ProductPriceEntity ToCreatePriceEntity(Guid productKey, ProductPrice productPrice, int revision = 1);
    ProductRevision ToProductRevision(ProductEntity productRevisionEntity);
    ProductPriceRevision ToProductPriceRevision(ProductPriceEntity productPriceEntity);
}