using System.Linq.Expressions;
using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.GetRevision;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetRevision;

public class GetRevisionManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetRevisionManager _sut;
    private readonly Guid _testProductKey;
    private readonly DateTime _testTimestamp;
    private readonly EntityLifeCycleResult<ProductEntity, ProductRevisionEntity> _testProductRevision;
    private readonly List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> _testPriceRevisions;

    public GetRevisionManagerTests()
    {
        _mocker = new AutoMocker();
        _testTimestamp = DateTime.UtcNow;
        _testProductKey = Guid.NewGuid();

        // Setup test data
        _testProductRevision = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
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
        };

        _testPriceRevisions = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
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
                    Action = RevisionAction.Created
                }
            }
        };

        // Setup happy paths
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetHistoricAsync(_testProductKey, _testTimestamp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProductRevision);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testPriceRevisions);

        _mocker.Use(new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>()).CreateMapper());

        _sut = _mocker.CreateInstance<GetRevisionManager>();
    }

    [Fact]
    public async Task GetRevisionAsync_ShouldReturnRevisionWithPrices()
    {
        // Act
        var result = await _sut.GetRevisionAsync(_testProductKey, _testTimestamp);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.Product);
        Assert.Equal(_testProductRevision.Entity.Name, result.Product?.Entity.Name);
        Assert.NotNull(result.ProductPrices);
        Assert.Single(result.ProductPrices!);
        Assert.Equal(_testPriceRevisions[0].Entity.Value, result.ProductPrices!.First().Entity.Value);
    }

    [Fact]
    public async Task GetRevisionAsync_WhenRevisionNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetHistoricAsync(_testProductKey, _testTimestamp, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>?)null);

        // Act
        var result = await _sut.GetRevisionAsync(_testProductKey, _testTimestamp);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
        Assert.Null(result.Product);
    }

    [Fact]
    public async Task GetRevisionAsync_WithProductError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Failed to get product revision");
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetHistoricAsync(_testProductKey, _testTimestamp, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _sut.GetRevisionAsync(_testProductKey, _testTimestamp, It.IsAny<CancellationToken>()));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task GetRevisionAsync_WithPriceError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Failed to get price revisions");
        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _sut.GetRevisionAsync(_testProductKey, _testTimestamp));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task GetRevisionAsync_WithDeletedRevision_ShouldReturnDeletedState()
    {
        // Arrange
        var deletedRevision = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = new ProductEntity { Name = "Deleted Product" },
            Revision = new ProductRevisionEntity
            {
                Key = _testProductKey,
                CreatedAt = _testTimestamp,
                Action = RevisionAction.Deleted
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetHistoricAsync(_testProductKey, _testTimestamp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedRevision);

        // Act
        var result = await _sut.GetRevisionAsync(_testProductKey, _testTimestamp);

        // Assert
        Assert.NotNull(result.Product);
        Assert.Equal(RevisionAction.Deleted, result.Product?.Revision.Action);
    }
}