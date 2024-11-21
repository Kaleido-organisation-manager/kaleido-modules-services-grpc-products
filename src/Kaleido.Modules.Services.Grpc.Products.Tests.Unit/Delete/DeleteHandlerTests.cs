using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.Delete;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Delete;

public class DeleteHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly DeleteHandler _sut;
    private readonly ProductRequest _testRequest;
    private readonly ProductResponse _testResponse;
    private readonly ManagerResponse _testManagerResponse;

    public DeleteHandlerTests()
    {
        _mocker = new AutoMocker();

        // Setup test data
        var productKey = Guid.NewGuid();
        _testRequest = new ProductRequest { Key = productKey.ToString() };

        var mappingConfig = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>());
        _mocker.Use(mappingConfig.CreateMapper());

        var productEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "https://test.com/image.png"
        };

        var revision = new ProductRevisionEntity
        {
            Key = productKey,
            CreatedAt = DateTime.UtcNow
        };

        var productResult = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = productEntity,
            Revision = revision
        };

        var priceEntity = new ProductPriceEntity
        {
            Units = 10,
            Nanos = 0,
            CurrencyKey = Guid.NewGuid(),
            ProductKey = productKey
        };

        var priceRevision = new ProductPriceRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var priceResult = new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
        {
            Entity = priceEntity,
            Revision = priceRevision
        };

        _testManagerResponse = new ManagerResponse(productResult, new[] { priceResult });

        var prices = new List<ProductPriceResponse>();

        var price = new ProductPriceResponse
        {
            Price = new ProductPrice { Units = priceEntity.Units, Nanos = priceEntity.Nanos, CurrencyKey = priceEntity.CurrencyKey.ToString() },
            Revision = new BaseRevision
            {
                Revision = 1,
                Key = priceRevision.Key.ToString(),
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(priceRevision.CreatedAt)
            }
        };

        prices.Add(price);

        _testResponse = new ProductResponse
        {
            Product = new ProductWithPricesResponse
            {
                Name = productEntity.Name,
                Description = productEntity.Description,
                CategoryKey = productEntity.CategoryKey.ToString(),
                ImageUrl = productEntity.ImageUrl,
                Prices = { prices }
            },
            Revision = new BaseRevision
            {
                Key = revision.Key.ToString(),
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(revision.CreatedAt)
            }
        };

        // Setup happy paths
        _mocker.Use(new KeyValidator());

        _mocker.GetMock<IDeleteManager>()
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testManagerResponse);

        _sut = _mocker.CreateInstance<DeleteHandler>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldReturnResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_testRequest);

        // Assert
        Assert.Equal(_testRequest.Key, result.Key);
        _mocker.GetMock<IDeleteManager>()
            .Verify(x => x.DeleteAsync(Guid.Parse(_testRequest.Key), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidKey_ShouldThrowRpcException()
    {
        // Arrange
        var invalidRequest = new ProductRequest { Key = "not-a-guid" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProduct_ShouldThrowRpcException()
    {
        // Arrange
        _mocker.GetMock<IDeleteManager>()
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse(ManagerResponseState.NotFound));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testRequest));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WithUnexpectedError_ShouldThrowRpcException()
    {
        // Arrange
        _mocker.GetMock<IDeleteManager>()
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}