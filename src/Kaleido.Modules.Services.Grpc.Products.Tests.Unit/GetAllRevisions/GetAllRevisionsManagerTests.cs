using System.Linq.Expressions;
using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.GetAllRevisions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllRevisions;

public class GetAllRevisionsManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllRevisionsManager _sut;
    private readonly Guid _testProductKey;
    private readonly DateTime _testTimestamp;
    private readonly List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>> _testProductRevisions;
    private readonly List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>> _testPriceRevisions;

    public GetAllRevisionsManagerTests()
    {
        _mocker = new AutoMocker();
        _testTimestamp = DateTime.UtcNow;
        _testProductKey = Guid.NewGuid();

        // Setup test data
        _testProductRevisions = new List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>
        {
            new()
            {
                Entity = new ProductEntity
                {
                    Name = "Test Product V1",
                    Description = "Initial Version",
                    CategoryKey = Guid.NewGuid()
                },
                Revision = new ProductRevisionEntity
                {
                    Key = _testProductKey,
                    CreatedAt = _testTimestamp.AddHours(-2),
                    Action = RevisionAction.Created,
                    Revision = 1
                }
            },
            new()
            {
                Entity = new ProductEntity
                {
                    Name = "Test Product V2",
                    Description = "Updated Version",
                    CategoryKey = Guid.NewGuid()
                },
                Revision = new ProductRevisionEntity
                {
                    Key = _testProductKey,
                    CreatedAt = _testTimestamp.AddHours(-1),
                    Action = RevisionAction.Updated,
                    Revision = 2
                }
            }
        };

        _testPriceRevisions = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
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
                    CreatedAt = _testTimestamp.AddHours(-2),
                    Action = RevisionAction.Created,
                    Revision = 1
                }
            },
            new()
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = 19,
                    Nanos = 99,
                    CurrencyKey = Guid.NewGuid()
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp.AddHours(-1),
                    Action = RevisionAction.Updated,
                    Revision = 2
                }
            }
        };

        // Setup happy paths
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAllAsync(_testProductKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testProductRevisions);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testPriceRevisions);

        _mocker.Use(new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>()).CreateMapper());

        _sut = _mocker.CreateInstance<GetAllRevisionsManager>();
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ShouldReturnAllRevisions()
    {
        // Act
        var result = await _sut.GetAllRevisionsAsync(_testProductKey);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(_testProductRevisions.Count, resultList.Count);
        Assert.Equal(_testProductRevisions[1].Entity.Name, resultList[0].Product?.Entity.Name);
        Assert.Equal(_testProductRevisions[0].Entity.Name, resultList[1].Product?.Entity.Name);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ShouldIncludeAssociatedPrices()
    {
        // Act
        var result = await _sut.GetAllRevisionsAsync(_testProductKey);

        // Assert
        var resultList = result.ToList();
        foreach (var revision in resultList)
        {
            Assert.NotNull(revision.ProductPrices);
            Assert.NotEmpty(revision.ProductPrices!);
        }
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ShouldOrderRevisionsByTimestamp()
    {
        // Act
        var result = await _sut.GetAllRevisionsAsync(_testProductKey);

        // Assert
        var resultList = result.ToList();
        Assert.True(resultList[0].Product?.Revision.CreatedAt > resultList[1].Product?.Revision.CreatedAt);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_WithNoRevisions_ShouldReturnEmptyList()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAllAsync(_testProductKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>());

        // Act
        var result = await _sut.GetAllRevisionsAsync(_testProductKey);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_WithProductError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Failed to get product revisions");
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAllAsync(_testProductKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllRevisionsAsync(_testProductKey));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_WithPriceError_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception("Failed to get price revisions");
        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllRevisionsAsync(_testProductKey));
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ShouldHandleDeletedRevisions()
    {
        // Arrange
        var deletedRevision = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = new ProductEntity { Name = "Deleted Product" },
            Revision = new ProductRevisionEntity
            {
                Key = _testProductKey,
                CreatedAt = _testTimestamp,
                Action = RevisionAction.Deleted,
                Revision = 3
            }
        };

        var allRevisions = _testProductRevisions.Concat(new[] { deletedRevision });

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAllAsync(_testProductKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allRevisions);

        // Act
        var result = await _sut.GetAllRevisionsAsync(_testProductKey);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.Contains(resultList, r => r.Product?.Revision.Action == RevisionAction.Deleted);
    }
}