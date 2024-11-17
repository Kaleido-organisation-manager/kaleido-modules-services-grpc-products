using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Create;

public class CreateManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly CreateManager _sut;
    private readonly ProductEntity _testProductEntity;
    private readonly ProductPrice[] _testProductPrices;
    private readonly DateTime _testTimestamp;

    public CreateManagerTests()
    {
        _mocker = new AutoMocker();
        _testTimestamp = DateTime.UtcNow;

        // Setup test data
        _testProductEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid()
        };

        _testProductPrices = new[]
        {
            new ProductPrice
            {
                Value = 10.0f,
                CurrencyKey = Guid.NewGuid().ToString()
            },
            new ProductPrice
            {
                Value = 15.0f,
                CurrencyKey = Guid.NewGuid().ToString()
            }
        };

        // Setup happy paths for product lifecycle handler
        var productRevision = new ProductRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = _testTimestamp
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.CreateAsync(_testProductEntity, It.IsAny<ProductRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity entity, ProductRevisionEntity revision, CancellationToken _) =>
                new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
                {
                    Entity = entity,
                    Revision = revision
                });

        // Setup happy paths for price lifecycle handler
        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.CreateAsync(It.IsAny<ProductPriceEntity>(), It.IsAny<ProductPriceRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductPriceEntity entity, ProductPriceRevisionEntity revision, CancellationToken _) =>
                new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
                {
                    Entity = entity,
                    Revision = revision
                });

        _sut = _mocker.CreateInstance<CreateManager>();
    }

    [Fact]
    public async Task CreateAsync_WithValidInputs_ShouldCreateProductAndPrices()
    {
        // Act
        var result = await _sut.CreateAsync(_testProductEntity, _testProductPrices);

        // Assert
        Assert.NotNull(result.Product);
        Assert.NotNull(result.ProductPrices);
        Assert.Equal(_testProductEntity, result.Product.Entity);
        Assert.Equal(_testProductPrices.Length, result.ProductPrices.Count());

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Verify(x => x.CreateAsync(_testProductEntity, It.IsAny<ProductRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.CreateAsync(It.IsAny<ProductPriceEntity>(), It.IsAny<ProductPriceRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(_testProductPrices.Length));
    }

    [Fact]
    public async Task CreateAsync_ShouldSetCorrectProductKeyForPrices()
    {
        // Act
        var result = await _sut.CreateAsync(_testProductEntity, _testProductPrices);

        // Assert
        foreach (var priceResult in result.ProductPrices ?? [])
        {
            Assert.NotNull(result.Product);
            Assert.Equal(result.Product.Revision.Key, priceResult.Entity.ProductKey);
        }
    }

    [Fact]
    public async Task CreateAsync_ShouldUseConsistentTimestamp()
    {
        // Act
        var result = await _sut.CreateAsync(_testProductEntity, _testProductPrices);

        // Assert
        var productTimestamp = result.Product?.Revision.CreatedAt;
        foreach (var priceResult in result.ProductPrices ?? [])
        {
            Assert.Equal(productTimestamp, priceResult.Revision.CreatedAt);
        }
    }

    [Fact]
    public async Task CreateAsync_WithProductCreationError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Product creation failed");
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<ProductRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(_testProductEntity, _testProductPrices));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithPriceCreationError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Price creation failed");
        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.CreateAsync(It.IsAny<ProductPriceEntity>(), It.IsAny<ProductPriceRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(_testProductEntity, _testProductPrices));
        Assert.Equal(expectedException.Message, exception.Message);
    }
}