using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Mappers;
public class ProductMapperTests
{
    private readonly ProductMapper _mapper = new ProductMapper();

    [Fact]
    public void FromEntities_ShouldMapAllFields()
    {
        // Arrange
        var productEntity = new ProductEntity
        {
            Key = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "http://example.com/image.jpg"
        };

        var productPriceEntities = new List<ProductPriceEntity>
        {
            new ProductPriceEntity
            {
                Price = 9.99f,
                CurrencyKey = Guid.NewGuid(),
                ProductKey = productEntity.Key
            }
        };

        // Act
        var result = _mapper.FromEntities(productEntity, productPriceEntities);

        // Assert
        Assert.Equal(productEntity.Key.ToString(), result.Key);
        Assert.Equal(productEntity.Name, result.Name);
        Assert.Equal(productEntity.Description, result.Description);
        Assert.Equal(productEntity.CategoryKey.ToString(), result.CategoryKey);
        Assert.Equal(productEntity.ImageUrl, result.ImageUrl);
        Assert.Single(result.Prices);
        Assert.Equal(productPriceEntities[0].Price, result.Prices[0].Value);
        Assert.Equal(productPriceEntities[0].CurrencyKey.ToString(), result.Prices[0].CurrencyKey);
    }

    [Fact]
    public void ToCreateEntity_ShouldMapAllFields()
    {
        // Arrange
        var product = new Product
        {
            Key = Guid.NewGuid().ToString(),
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid().ToString(),
            ImageUrl = "http://example.com/image.jpg"
        };

        // Act
        var result = _mapper.ToCreateEntity(product);

        // Assert
        Assert.Equal(Guid.Parse(product.Key), result.Key);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(Guid.Parse(product.CategoryKey), result.CategoryKey);
        Assert.Equal(product.ImageUrl, result.ImageUrl);
        Assert.Equal(EntityStatus.Active, result.Status);
        Assert.Equal(1, result.Revision);
        Assert.True((DateTime.UtcNow - result.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void ToCreatePriceEntity_ShouldMapAllFields()
    {
        // Arrange
        var productKey = Guid.NewGuid();
        var productPrice = new ProductPrice
        {
            Value = 9.99f,
            CurrencyKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = _mapper.ToCreatePriceEntity(productKey, productPrice);

        // Assert
        Assert.Equal(productKey, result.Key);
        Assert.Equal(productKey, result.ProductKey);
        Assert.Equal(productPrice.Value, result.Price);
        Assert.Equal(Guid.Parse(productPrice.CurrencyKey), result.CurrencyKey);
        Assert.Equal(EntityStatus.Active, result.Status);
        Assert.Equal(1, result.Revision);
        Assert.True((DateTime.UtcNow - result.CreatedAt).TotalSeconds < 1);
    }

    [Fact]
    public void ToProductRevision_ShouldMapAllFields()
    {
        // Arrange
        var productEntity = new ProductEntity
        {
            Key = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "http://example.com/image.jpg",
            Revision = 2,
            Status = EntityStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = _mapper.ToProductRevision(productEntity);

        // Assert
        Assert.Equal(productEntity.Key.ToString(), result.Key);
        Assert.Equal(productEntity.Name, result.Name);
        Assert.Equal(productEntity.Description, result.Description);
        Assert.Equal(productEntity.CategoryKey.ToString(), result.CategoryKey);
        Assert.Equal(productEntity.ImageUrl, result.ImageUrl);
        Assert.Equal(productEntity.Revision, result.Revision);
        Assert.Equal(productEntity.Status.ToString(), result.Status);
        Assert.Equal(productEntity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"), result.CreatedAt);
    }

    [Fact]
    public void ToProductPriceRevision_ShouldMapAllFields()
    {
        // Arrange
        var productPriceEntity = new ProductPriceEntity
        {
            Price = 9.99f,
            CurrencyKey = Guid.NewGuid(),
            Revision = 2,
            CreatedAt = DateTime.UtcNow,
            Status = EntityStatus.Active,
            ProductKey = Guid.NewGuid()
        };

        // Act
        var result = _mapper.ToProductPriceRevision(productPriceEntity);

        // Assert
        Assert.Equal(productPriceEntity.Price, result.Value);
        Assert.Equal(productPriceEntity.CurrencyKey.ToString(), result.CurrencyKey);
        Assert.Equal(productPriceEntity.Revision, result.Revision);
        Assert.Equal(productPriceEntity.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"), result.CreatedAt);
        Assert.Equal(productPriceEntity.Status.ToString(), result.Status);
    }
}
