using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Mappers;

public class ProductMappingProfileTests
{
    private readonly IMapper _mapper;

    public ProductMappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappingProfile>();
        });

        configuration.AssertConfigurationIsValid();
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Map_FromProductToProductEntity_ShouldMapCorrectly()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid().ToString(),
            ImageUrl = "test-image-url"
        };

        // Act
        var result = _mapper.Map<ProductEntity>(product);

        // Assert
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(Guid.Parse(product.CategoryKey), result.CategoryKey);
        Assert.Equal(product.ImageUrl, result.ImageUrl);
    }

    [Fact]
    public void Map_FromProductEntityToProductWithPrices_ShouldMapCorrectly()
    {
        // Arrange
        var productEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "test-image-url"
        };

        // Act
        var result = _mapper.Map<ProductWithPrices>(productEntity);

        // Assert
        Assert.Equal(productEntity.Name, result.Name);
        Assert.Equal(productEntity.Description, result.Description);
        Assert.Equal(productEntity.CategoryKey, result.CategoryKey);
        Assert.Equal(productEntity.ImageUrl, result.ImageUrl);
    }

    [Fact]
    public void Map_FromProductWithPricesToProductResponse_ShouldMapCorrectly()
    {
        // Arrange
        var productWithPrices = new ProductWithPrices
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "test-image-url",
            Prices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>()
        };
        var revision = new BaseRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var lifeCycleResult = new EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>
        {
            Entity = productWithPrices,
            Revision = revision
        };

        // Act
        var result = _mapper.Map<ProductResponse>(lifeCycleResult);

        // Assert
        Assert.Equal(productWithPrices.Name, result.Product.Name);
        Assert.Equal(productWithPrices.Description, result.Product.Description);
        Assert.Equal(revision.Key.ToString(), result.Revision.Key);
        Assert.Equal(Timestamp.FromDateTime(revision.CreatedAt), result.Revision.CreatedAt);
    }

    [Fact]
    public void Map_FromProductPriceEntityToProductPriceResponse_ShouldMapCorrectly()
    {
        // Arrange
        var productPriceEntity = new ProductPriceEntity
        {
            Units = 10,
            Nanos = 0,
            CurrencyKey = Guid.NewGuid(),
            ProductKey = Guid.NewGuid()
        };
        var revision = new ProductPriceRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var lifeCycleResult = new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
        {
            Entity = productPriceEntity,
            Revision = revision
        };

        // Act
        var result = _mapper.Map<ProductPriceResponse>(lifeCycleResult);

        // Assert
        Assert.Equal(productPriceEntity.Units, result.Price.Units);
        Assert.Equal(productPriceEntity.Nanos, result.Price.Nanos);
        Assert.Equal(productPriceEntity.CurrencyKey.ToString(), result.Price.CurrencyKey);
        Assert.Equal(revision.Key.ToString(), result.Revision.Key);
        Assert.Equal(Timestamp.FromDateTime(revision.CreatedAt), result.Revision.CreatedAt);
    }

    [Fact]
    public void Map_FromProductPriceEntityToProductPrice_ShouldMapCorrectly()
    {
        // Arrange
        var productPriceEntity = new ProductPriceEntity
        {
            Units = 10,
            Nanos = 0,
            CurrencyKey = Guid.NewGuid(),
            ProductKey = Guid.NewGuid()
        };

        // Act
        var result = _mapper.Map<ProductPrice>(productPriceEntity);

        // Assert
        Assert.Equal(productPriceEntity.Units, result.Units);
        Assert.Equal(productPriceEntity.Nanos, result.Nanos);
        Assert.Equal(productPriceEntity.CurrencyKey.ToString(), result.CurrencyKey);
    }

    [Fact]
    public void Map_FromProductWithPricesToProductWithPricesResponse_ShouldMapCorrectly()
    {
        // Arrange
        var productWithPrices = new ProductWithPrices
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "test-image-url",
            Prices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>()
        };

        // Act
        var result = _mapper.Map<ProductWithPricesResponse>(productWithPrices);

        // Assert
        Assert.Equal(productWithPrices.Name, result.Name);
        Assert.Equal(productWithPrices.Description, result.Description);
        Assert.Equal(productWithPrices.CategoryKey.ToString(), result.CategoryKey);
        Assert.Equal(productWithPrices.ImageUrl, result.ImageUrl);
    }

    [Fact]
    public void Map_FromTimestampToDateTime_ShouldMapCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var timestamp = Timestamp.FromDateTime(now);

        // Act
        var result = _mapper.Map<DateTime>(timestamp);

        // Assert
        Assert.Equal(now, result, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Map_FromDateTimeToTimestamp_ShouldMapCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var result = _mapper.Map<Timestamp>(now);

        // Assert
        Assert.Equal(now, result.ToDateTime(), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Map_FromProductEntityLifeCycleResultToProductWithPricesLifeCycleResult_ShouldMapCorrectly()
    {
        // Arrange
        var productEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "test-image-url"
        };
        var revision = new ProductRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var lifeCycleResult = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = productEntity,
            Revision = revision
        };

        // Act
        var result = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(lifeCycleResult);

        // Assert
        Assert.Equal(productEntity.Name, result.Entity.Name);
        Assert.Equal(productEntity.Description, result.Entity.Description);
        Assert.Equal(productEntity.CategoryKey, result.Entity.CategoryKey);
        Assert.Equal(productEntity.ImageUrl, result.Entity.ImageUrl);
        Assert.Equal(revision.Key, result.Revision.Key);
        Assert.Equal(revision.CreatedAt, result.Revision.CreatedAt);
    }

    [Fact]
    public void Map_SelfMappingProductPriceEntityLifeCycleResult_ShouldMapCorrectly()
    {
        // Arrange
        var productPriceEntity = new ProductPriceEntity
        {
            Units = 10,
            Nanos = 0,
            CurrencyKey = Guid.NewGuid(),
            ProductKey = Guid.NewGuid()
        };
        var revision = new ProductPriceRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var lifeCycleResult = new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
        {
            Entity = productPriceEntity,
            Revision = revision
        };

        // Act
        var result = _mapper.Map<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>(lifeCycleResult);

        // Assert
        Assert.Equal(productPriceEntity.Units, result.Entity.Units);
        Assert.Equal(productPriceEntity.Nanos, result.Entity.Nanos);
        Assert.Equal(productPriceEntity.CurrencyKey, result.Entity.CurrencyKey);
        Assert.Equal(productPriceEntity.ProductKey, result.Entity.ProductKey);
        Assert.Equal(revision.Key, result.Revision.Key);
        Assert.Equal(revision.CreatedAt, result.Revision.CreatedAt);
    }

    [Fact]
    public void Map_SelfMappingProductEntityLifeCycleResult_ShouldMapCorrectly()
    {
        // Arrange
        var productEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "test-image-url"
        };
        var revision = new ProductRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        var lifeCycleResult = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = productEntity,
            Revision = revision
        };

        // Act
        var result = _mapper.Map<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>(lifeCycleResult);

        // Assert
        Assert.Equal(productEntity.Name, result.Entity.Name);
        Assert.Equal(productEntity.Description, result.Entity.Description);
        Assert.Equal(productEntity.CategoryKey, result.Entity.CategoryKey);
        Assert.Equal(productEntity.ImageUrl, result.Entity.ImageUrl);
        Assert.Equal(revision.Key, result.Revision.Key);
        Assert.Equal(revision.CreatedAt, result.Revision.CreatedAt);
    }
}