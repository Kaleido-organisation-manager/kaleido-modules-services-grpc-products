using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Delete;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Delete;

public class DeleteManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly DeleteManager _sut;
    private readonly Guid _testProductKey;
    private readonly DateTime _testTimestamp;
    private readonly ProductEntity _testProductEntity;
    private readonly List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> _testProductPrices;

    public DeleteManagerTests()
    {
        _mocker = new AutoMocker();
        _testTimestamp = DateTime.UtcNow;
        _testProductKey = Guid.NewGuid();

        // Setup test data
        _testProductEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid()
        };

        var productRevision = new ProductRevisionEntity
        {
            Key = _testProductKey,
            CreatedAt = _testTimestamp
        };

        _testProductPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
        {
            new()
            {
                Entity = new ProductPriceEntity
                {
                    Value = 10.0f,
                    CurrencyKey = Guid.NewGuid(),
                    ProductKey = _testProductKey
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            },
            new()
            {
                Entity = new ProductPriceEntity
                {
                    Value = 15.0f,
                    CurrencyKey = Guid.NewGuid(),
                    ProductKey = _testProductKey
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            }
        };

        // Setup happy paths
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAsync(_testProductKey, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
            {
                Entity = _testProductEntity,
                Revision = productRevision
            });

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProductPrices);

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.DeleteAsync(_testProductKey, It.IsAny<ProductRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid key, ProductRevisionEntity revision, CancellationToken _) =>
                new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
                {
                    Entity = _testProductEntity,
                    Revision = revision
                });

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<ProductPriceRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid key, ProductPriceRevisionEntity revision, CancellationToken _) =>
                new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
                {
                    Entity = _testProductPrices.First(p => p.Revision.Key == key).Entity,
                    Revision = revision
                });

        _sut = _mocker.CreateInstance<DeleteManager>();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingProduct_ShouldDeleteProductAndPrices()
    {
        // Act
        var result = await _sut.DeleteAsync(_testProductKey);

        // Assert
        Assert.NotNull(result.Product);
        Assert.NotNull(result.ProductPrices);
        Assert.Equal(_testProductEntity, result.Product.Entity);
        Assert.Equal(_testProductPrices.Count, result.ProductPrices.Count());

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Verify(x => x.DeleteAsync(_testProductKey, It.IsAny<ProductRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<ProductPriceRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(_testProductPrices.Count));
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentProduct_ShouldReturnEmptyResponse()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAsync(_testProductKey, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>?)null);

        // Act
        var result = await _sut.DeleteAsync(_testProductKey);

        // Assert
        Assert.Null(result.Product);
        Assert.Null(result.ProductPrices);

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<ProductRevisionEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldUseConsistentTimestamp()
    {
        // Act
        var result = await _sut.DeleteAsync(_testProductKey);

        // Assert
        var productTimestamp = result.Product?.Revision.CreatedAt;
        foreach (var priceResult in result.ProductPrices ?? [])
        {
            Assert.Equal(productTimestamp, priceResult.Revision.CreatedAt);
        }
    }

    [Fact]
    public async Task DeleteAsync_WithProductDeletionError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Product deletion failed");
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<ProductRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(_testProductKey));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_WithPriceDeletionError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Price deletion failed");
        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<ProductPriceRevisionEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(_testProductKey));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldFilterOutDeletedPrices()
    {
        // Arrange
        var deletedPrice = new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
        {
            Entity = new ProductPriceEntity
            {
                Value = 20.0f,
                CurrencyKey = Guid.NewGuid(),
                ProductKey = _testProductKey
            },
            Revision = new ProductPriceRevisionEntity
            {
                Key = Guid.NewGuid(),
                CreatedAt = _testTimestamp,
                Action = RevisionAction.Deleted
            }
        };

        var allPrices = _testProductPrices.Concat(new[] { deletedPrice });

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allPrices);

        // Act
        var result = await _sut.DeleteAsync(_testProductKey);

        // Assert
        Assert.Equal(_testProductPrices.Count, result.ProductPrices?.Count() ?? 0);
        Assert.DoesNotContain(result.ProductPrices ?? [], p => p.Revision.Key == deletedPrice.Revision.Key);
    }
}