using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.GetAll;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAll;

public class GetAllManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllManager _sut;
    private readonly DateTime _testTimestamp;
    private readonly List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>> _testProducts;
    private readonly List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> _testPrices;
    private readonly Guid _testProductKey;

    public GetAllManagerTests()
    {
        _mocker = new AutoMocker();
        _testTimestamp = DateTime.UtcNow;
        _testProductKey = Guid.NewGuid();

        // Setup test data
        _testProducts = new List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>
        {
            new()
            {
                Entity = new ProductEntity
                {
                    Name = "Test Product",
                    Description = "Test Description",
                    CategoryKey = Guid.NewGuid()
                },
                Revision = new ProductRevisionEntity
                {
                    Key = _testProductKey,
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            }
        };

        _testPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
        {
            new()
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = 9,
                    Nanos = 99,
                    CurrencyKey = Guid.NewGuid()
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created,
                    Revision = 1
                }
            }
        };

        // Setup happy paths
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProducts);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testPrices);

        _sut = _mocker.CreateInstance<GetAllManager>();
    }

    [Fact]
    public async Task GetAllProductsAsync_WithValidInputs_ShouldReturnProductsAndPrices()
    {
        // Act
        var result = await _sut.GetAllProductsAsync(CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.NotNull(resultList[0].Product);
        Assert.Equal(_testProductKey, resultList[0].Product!.Revision.Key);
        Assert.Equal("Test Product", resultList[0].Product!.Entity.Name);
        Assert.NotNull(resultList[0].ProductPrices);
        Assert.Single(resultList[0].ProductPrices!);
        Assert.Equal(9, resultList[0].ProductPrices!.First().Entity.Units);
        Assert.Equal(99, resultList[0].ProductPrices!.First().Entity.Nanos);

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Verify(x => x.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithNoProducts_ShouldReturnEmptyList()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>());

        // Act
        var result = await _sut.GetAllProductsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Verify(x => x.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithProductsError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Failed to get products");
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllProductsAsync(CancellationToken.None));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithPricesError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Failed to get prices");
        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllProductsAsync(CancellationToken.None));
        Assert.Equal(expectedException.Message, exception.Message);
    }
}
