using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Get;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Get;

public class GetManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetManager _sut;
    private readonly Guid _testProductKey;
    private readonly DateTime _testTimestamp;
    private readonly ProductEntity _testProductEntity;
    private readonly List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> _testProductPrices;

    public GetManagerTests()
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
                    Units = 10,
                    Nanos = 0,
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

        _sut = _mocker.CreateInstance<GetManager>();
    }

    [Fact]
    public async Task GetAsync_WithExistingProduct_ShouldReturnProductAndPrices()
    {
        // Act
        var result = await _sut.GetAsync(_testProductKey);

        // Assert
        Assert.NotNull(result.Product);
        Assert.NotNull(result.ProductPrices);
        Assert.Equal(_testProductEntity, result.Product.Entity);
        Assert.Equal(_testProductPrices.Count, result.ProductPrices.Count());

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Verify(x => x.GetAsync(_testProductKey, It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.FindAllAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentProduct_ShouldReturnNotFoundState()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAsync(_testProductKey, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>?)null);

        // Act
        var result = await _sut.GetAsync(_testProductKey);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
        Assert.Null(result.Product);
        Assert.Null(result.ProductPrices);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.FindAllAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_WithProductGetError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Product get failed");
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAsync(_testProductKey));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task GetAsync_WithPriceGetError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Price get failed");
        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAsync(_testProductKey));
        Assert.Equal(expectedException.Message, exception.Message);
    }
}
