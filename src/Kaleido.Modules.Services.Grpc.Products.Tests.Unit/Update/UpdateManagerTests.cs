using System.Linq.Expressions;
using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Update;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Update;

public class UpdateManagerTests
{
    private readonly AutoMocker _mocker;
    private UpdateManager _sut;
    private readonly Guid _testProductKey;
    private readonly DateTime _testTimestamp;
    private readonly ProductEntity _testProduct;
    private readonly List<ProductPriceEntity> _testPrices;

    public UpdateManagerTests()
    {
        _mocker = new AutoMocker();
        _testTimestamp = DateTime.UtcNow;
        _testProductKey = Guid.NewGuid();

        // Setup test data
        _testProduct = new ProductEntity
        {
            Name = "Updated Product",
            Description = "Updated Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "https://example.com/image.jpg"
        };

        _testPrices = new List<ProductPriceEntity>
        {
            new()
            {
                ProductKey = _testProductKey,
                Units = 29,
                Nanos = 99,
                CurrencyKey = Guid.NewGuid()
            }
        };

        var productResult = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = _testProduct,
            Revision = new ProductRevisionEntity
            {
                Key = _testProductKey,
                CreatedAt = _testTimestamp,
                Action = RevisionAction.Updated
            }
        };

        // Setup happy paths
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductEntity>(),
                It.IsAny<ProductRevisionEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(productResult);

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productResult);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductPriceEntity>(),
                It.IsAny<ProductPriceRevisionEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid key, ProductPriceEntity entity, ProductPriceRevisionEntity revision, CancellationToken cancellationToken) =>
            {
                return new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
                {
                    Entity = entity,
                    Revision = revision
                };
            });

        _mocker.Use(new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>()).CreateMapper());

        _sut = _mocker.CreateInstance<UpdateManager>();
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldReturnUpdatedProduct()
    {
        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, _testPrices);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.Product);
        Assert.Equal(_testProduct.Name, result.Product?.Entity.Name);
        Assert.Equal(RevisionAction.Updated, result.Product?.Revision.Action);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductEntity>(),
                It.IsAny<ProductRevisionEntity>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotModifiedException($"No changes."));

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>?)null);

        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, _testPrices);

        // Assert
        Assert.Equal(ManagerResponseState.NotFound, result.State);
    }

    [Fact]
    public async Task UpdateAsync_WithNoChanges_ShouldReturnUnmodifiedProduct()
    {
        // Arrange
        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductEntity>(),
                It.IsAny<ProductRevisionEntity>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotModifiedException("No changes."));

        var existingProduct = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = _testProduct,
            Revision = new ProductRevisionEntity
            {
                Key = _testProductKey,
                CreatedAt = _testTimestamp,
                Action = RevisionAction.Created
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity>>()
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, _testPrices);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.Product);
        Assert.Equal(RevisionAction.Created, result.Product?.Revision.Action);
    }

    [Fact]
    public async Task UpdateAsync_WithPriceChanges_ShouldHandlePriceUpdates()
    {
        // Arrange
        var existingPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
        {
            new()
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = 19,
                    Nanos = 99,
                    CurrencyKey = _testPrices[0].CurrencyKey
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPrices);

        _sut = _mocker.CreateInstance<UpdateManager>();

        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, _testPrices);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.ProductPrices);
        Assert.NotEmpty(result.ProductPrices!);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductPriceEntity>(),
                It.IsAny<ProductPriceRevisionEntity>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithDeletedPrices_ShouldHandlePriceDeletions()
    {
        // Arrange
        var existingPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
        {
            new()
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = 19,
                    Nanos = 99,
                    CurrencyKey = Guid.NewGuid() // Different currency key
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPrices);

        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, _testPrices);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.ProductPrices);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.DeleteAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductPriceRevisionEntity>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithPriceValueUpdate_ShouldHandlePriceValueChanges()
    {
        // Arrange
        var existingPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
        {
            new()
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = 19,
                    Nanos = 99,
                    CurrencyKey = _testPrices[0].CurrencyKey
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPrices);

        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, _testPrices);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.ProductPrices);
        Assert.NotEmpty(result.ProductPrices!);
        Assert.Contains(result.ProductPrices!.Select(p => p.Entity), p => p.Units == _testPrices[0].Units && p.Nanos == _testPrices[0].Nanos);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.Is<ProductPriceEntity>(p => p.Units == _testPrices[0].Units && p.Nanos == _testPrices[0].Nanos),
                It.IsAny<ProductPriceRevisionEntity>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithDeletedPriceRestore_ShouldHandlePriceRestoration()
    {
        // Arrange
        var existingPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
        {
            new()
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = _testPrices[0].Units,
                    Nanos = _testPrices[0].Nanos,
                    CurrencyKey = _testPrices[0].CurrencyKey
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Deleted
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPrices);

        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, _testPrices);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.ProductPrices);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.RestoreAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductPriceRevisionEntity>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithUnchangedPrices_ShouldMarkPricesAsUnmodified()
    {
        // Arrange
        var existingPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
        {
            new()
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = _testPrices[0].Units,
                    Nanos = _testPrices[0].Nanos,
                    CurrencyKey = _testPrices[0].CurrencyKey
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPrices);

        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, _testPrices);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.ProductPrices);
        Assert.Contains(result.ProductPrices!, p => p.Revision.Action == RevisionAction.Unmodified);
    }

    [Fact]
    public async Task UpdateAsync_WithMultipleOperations_ShouldHandleAllPriceChanges()
    {
        // Arrange
        var existingPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>
        {
            new() // Price to be deleted
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = 15,
                    Nanos = 99,
                    CurrencyKey = Guid.NewGuid()
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            },
            new() // Price to be updated
            {
                Entity = new ProductPriceEntity
                {
                    ProductKey = _testProductKey,
                    Units = 19,
                    Nanos = 99,
                    CurrencyKey = _testPrices[0].CurrencyKey
                },
                Revision = new ProductPriceRevisionEntity
                {
                    Key = Guid.NewGuid(),
                    CreatedAt = _testTimestamp,
                    Action = RevisionAction.Created
                }
            }
        };

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Setup(x => x.FindAllAsync(
                It.IsAny<Expression<Func<ProductPriceEntity, bool>>>(),
                It.IsAny<Expression<Func<ProductPriceRevisionEntity, bool>>>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPrices);

        var newPrices = new List<ProductPriceEntity>
        {
            _testPrices[0], // Update existing price
            new() // Create new price
            {
                ProductKey = _testProductKey,
                Units = 39,
                Nanos = 99,
                CurrencyKey = Guid.NewGuid()
            }
        };

        // Act
        var result = await _sut.UpdateAsync(_testProductKey, _testProduct, newPrices);

        // Assert
        Assert.Equal(ManagerResponseState.Success, result.State);
        Assert.NotNull(result.ProductPrices);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.DeleteAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductPriceRevisionEntity>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductPriceEntity>(),
                It.IsAny<ProductPriceRevisionEntity>(),
                It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .Verify(x => x.CreateAsync(
                It.IsAny<ProductPriceEntity>(),
                It.IsAny<ProductPriceRevisionEntity>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }
}