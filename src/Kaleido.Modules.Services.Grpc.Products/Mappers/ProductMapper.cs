using Kaleido.Modules.Services.Grpc.Products.Constants;
using Kaleido.Modules.Services.Grpc.Products.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Mappers;

public class ProductMapper : IProductMapper
{
    public Product FromEntities(ProductEntity productEntity, IEnumerable<ProductPriceEntity> productPriceEntities)
    {
        return new Product
        {
            Key = productEntity.Key,
            Name = productEntity.Name,
            Description = productEntity.Description,
            Prices = {productPriceEntities.Select(x => new ProductPrice
            {
                Value = x.Price,
                CurrencyKey = x.CurrencyKey
            }).ToList()}
        };
    }

    public ProductEntity ToCreateEntity(Product product, int revision = 1)
    {
        return new ProductEntity
        {
            Key = Guid.NewGuid().ToString(),
            Name = product.Name,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = EntityStatus.Active,
            Revision = revision,
            Description = product.Description,
            CategoryKey = product.CategoryKey
        };
    }

    public ProductPriceEntity ToCreatePriceEntity(string productKey, ProductPrice productPrice, int revision = 1)
    {
        return new ProductPriceEntity
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Key = productKey,
            Revision = 1,
            Status = EntityStatus.Active,
            ProductKey = productKey,
            Price = productPrice.Value,
            CurrencyKey = productPrice.CurrencyKey
        };
    }
}