using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Mappers;

public class ProductMapper : IProductMapper
{
    public Product FromEntities(ProductEntity productEntity, IEnumerable<ProductPriceEntity> productPriceEntities)
    {
        return new Product
        {
            Key = productEntity.Key.ToString(),
            Name = productEntity.Name,
            Description = productEntity.Description,
            CategoryKey = productEntity.CategoryKey.ToString(),
            ImageUrl = productEntity.ImageUrl,
            Prices = {productPriceEntities.Select(x => new ProductPrice
            {
                Value = x.Price,
                CurrencyKey = x.CurrencyKey.ToString()
            }).ToList()}
        };
    }

    public ProductEntity ToCreateEntity(Product product, int revision = 1)
    {
        var productKey = Guid.Parse(product.Key);
        if (productKey == Guid.Empty)
        {
            productKey = Guid.NewGuid();
        }

        return new ProductEntity
        {
            Key = productKey,
            Name = product.Name,
            CreatedAt = DateTime.UtcNow,
            Status = EntityStatus.Active,
            Revision = revision,
            Description = product.Description,
            CategoryKey = Guid.Parse(product.CategoryKey),
            ImageUrl = product.ImageUrl
        };
    }

    public ProductPriceEntity ToCreatePriceEntity(Guid productKey, ProductPrice productPrice, int revision = 1)
    {
        return new ProductPriceEntity
        {
            CreatedAt = DateTime.UtcNow,
            Key = productKey,
            Revision = revision,
            Status = EntityStatus.Active,
            ProductKey = productKey,
            Price = productPrice.Value,
            CurrencyKey = Guid.Parse(productPrice.CurrencyKey)
        };
    }

    public ProductRevision ToProductRevision(ProductEntity productRevisionEntity)
    {
        return new ProductRevision
        {
            Key = productRevisionEntity.Key.ToString(),
            Name = productRevisionEntity.Name,
            Description = productRevisionEntity.Description,
            CategoryKey = productRevisionEntity.CategoryKey.ToString(),
            ImageUrl = productRevisionEntity.ImageUrl,
            Revision = productRevisionEntity.Revision,
            Status = productRevisionEntity.Status.ToString(),
            CreatedAt = productRevisionEntity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }

    public ProductPriceRevision ToProductPriceRevision(ProductPriceEntity productPriceEntity)
    {
        return new ProductPriceRevision
        {
            Value = productPriceEntity.Price,
            CurrencyKey = productPriceEntity.CurrencyKey.ToString(),
            Revision = productPriceEntity.Revision,
            CreatedAt = productPriceEntity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Status = productPriceEntity.Status.ToString(),
        };
    }
}
