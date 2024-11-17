using System.Linq.Expressions;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.GetAllFiltered;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllFiltered;

public class GetAllFilteredManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllFilteredManager _sut;
    private readonly DateTime _testTimestamp;
    private readonly List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>> _testProducts;
    private readonly List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> _testPrices;
    private readonly Guid _testProductKey;
    private readonly Guid _testCategoryKey;
    private readonly string _testProductName;

    public GetAllFilteredManagerTests()
    {
        _mocker = new AutoMocker();
        _testTimestamp = DateTime.UtcNow;
        _testProductKey = Guid.NewGuid();
        _testCategoryKey = Guid.NewGuid();
        _testProductName = "Test Product";

        // Setup test data
        _testProducts = new List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>
        {
            new()
            {
                Entity = new ProductEntity
                {
                    Name = _testProductName,
                    Description = "Test Description",
                    CategoryKey = _testCategoryKey
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
                    Value = 9.99f,
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
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProducts);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testPrices);

        _sut = _mocker.CreateInstance<GetAllFilteredManager>();
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithBothFilters_ShouldReturnFilteredProducts()
    {
        // Act
        var result = await _sut.GetAllFilteredAsync(
            "Test",
            _testCategoryKey.ToString(),
            CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.NotNull(resultList[0].Product);
        Assert.Equal(_testProductKey, resultList[0].Product!.Revision.Key);
        Assert.Equal(_testProductName, resultList[0].Product!.Entity.Name);
        Assert.Equal(_testCategoryKey, resultList[0].Product!.Entity.CategoryKey);
        Assert.NotNull(resultList[0].ProductPrices);
        Assert.Single(resultList[0].ProductPrices!);
        Assert.Equal(9.99f, resultList[0].ProductPrices!.First().Entity.Value);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithOnlyName_ShouldFilterByName()
    {
        // Act
        var result = await _sut.GetAllFilteredAsync(
            "Test",
            null,
            CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(_testProductName, resultList[0].Product!.Entity.Name);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithOnlyCategoryKey_ShouldFilterByCategory()
    {
        // Act
        var result = await _sut.GetAllFilteredAsync(
            null,
            _testCategoryKey.ToString(),
            CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(_testCategoryKey, resultList[0].Product!.Entity.CategoryKey);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithNoFilters_ShouldReturnAllProducts()
    {
        // Act
        var result = await _sut.GetAllFilteredAsync(
            null,
            null,
            CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Single(resultList);
        Assert.Equal(_testProductName, resultList[0].Product!.Entity.Name);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithDeletedProducts_ShouldFilterOutDeletedProducts()
    {
        // Arrange
        var deletedProduct = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = new ProductEntity { Name = "Deleted Product" },
            Revision = new ProductRevisionEntity { Action = RevisionAction.Deleted }
        };
        var products = _testProducts.Concat(new[] { deletedProduct });

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllFilteredAsync(
            "Test",
            _testCategoryKey.ToString(),
            CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(_testProductName, result.First().Product!.Entity.Name);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithDeletedPrices_ShouldFilterOutDeletedPrices()
    {
        // Arrange
        var deletedPrice = new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
        {
            Entity = new ProductPriceEntity { ProductKey = _testProductKey },
            Revision = new ProductPriceRevisionEntity { Action = RevisionAction.Deleted }
        };
        var prices = _testPrices.Concat(new[] { deletedPrice });

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(prices);

        // Act
        var result = await _sut.GetAllFilteredAsync(
            "Test",
            _testCategoryKey.ToString(),
            CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Single(result.First().ProductPrices!);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>());

        // Act
        var result = await _sut.GetAllFilteredAsync(
            "NonExistent",
            Guid.NewGuid().ToString(),
            CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WhenExceptionOccurs_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Test exception");
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _sut.GetAllFilteredAsync("Test", _testCategoryKey.ToString(), CancellationToken.None));
        Assert.Equal(expectedException.Message, exception.Message);
    }
}