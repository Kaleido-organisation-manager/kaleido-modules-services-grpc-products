using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Mappers.Interfaces;

public interface IProductMapper
{
    Product FromEntities(ProductEntity productEntity, IEnumerable<ProductPriceEntity> productPriceEntities);
    ProductEntity ToCreateEntity(Product product, int revision = 1);
    ProductPriceEntity ToCreatePriceEntity(string productId, ProductPrice productPrice, int revision = 1);
}