using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.GetRevision;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetRevision;

public class GetRevisionHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetRevisionHandler _sut;
    private readonly ProductRevisionRequest _testRequest;
    private readonly ManagerResponse _testManagerResponse;
    private readonly ProductResponse _testResponse;

    public GetRevisionHandlerTests()
    {
        _mocker = new AutoMocker();

        // Setup test data
        var productKey = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        _testRequest = new ProductRevisionRequest
        {
            Key = productKey.ToString(),
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(createdAt)
        };

        var productEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "https://example.com/image.jpg"
        };

        var productRevision = new ProductRevisionEntity
        {
            Key = productKey,
            CreatedAt = createdAt,
            Action = RevisionAction.Created
        };

        var productResult = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = productEntity,
            Revision = productRevision
        };

        var priceEntity = new ProductPriceEntity
        {
            Units = 9,
            Nanos = 99,
            CurrencyKey = Guid.NewGuid(),
            ProductKey = productKey
        };

        var priceRevision = new ProductPriceRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = createdAt,
            Action = RevisionAction.Created
        };

        var priceResult = new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
        {
            Entity = priceEntity,
            Revision = priceRevision
        };

        _testManagerResponse = new ManagerResponse(productResult, new[] { priceResult });

        _testResponse = new ProductResponse
        {
            Product = new ProductWithPricesResponse
            {
                Name = productEntity.Name,
                Description = productEntity.Description,
                CategoryKey = productEntity.CategoryKey.ToString(),
                Prices =
                {
                    new ProductPriceResponse
                    {
                        Price = new ProductPrice
                        {
                            Units = priceEntity.Units,
                            Nanos = priceEntity.Nanos,
                            CurrencyKey = priceEntity.CurrencyKey.ToString()
                        }
                    }
                }
            },
            Revision = new BaseRevision
            {
                Key = productRevision.Key.ToString(),
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(productRevision.CreatedAt)
            }
        };

        // Setup happy paths
        _mocker.GetMock<IGetRevisionManager>()
            .Setup(x => x.GetRevisionAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testManagerResponse);

        _mocker.Use(new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>()).CreateMapper());
        _mocker.Use(new KeyValidator());
        _mocker.Use(NullLogger<GetRevisionHandler>.Instance);

        _sut = _mocker.CreateInstance<GetRevisionHandler>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldReturnResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_testRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testResponse.Product.Name, result.Product.Name);
        Assert.Equal(_testResponse.Product.Description, result.Product.Description);
        Assert.Equal(_testResponse.Product.CategoryKey, result.Product.CategoryKey);
        Assert.Single(result.Product.Prices);
        Assert.Equal(_testResponse.Product.Prices[0].Price.Units, result.Product.Prices[0].Price.Units);
        Assert.Equal(_testResponse.Product.Prices[0].Price.Nanos, result.Product.Prices[0].Price.Nanos);

        _mocker.GetMock<IGetRevisionManager>()
            .Verify(x => x.GetRevisionAsync(
                Guid.Parse(_testRequest.Key),
                _testRequest.CreatedAt.ToDateTime(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    public async Task HandleAsync_WithInvalidKey_ShouldThrowRpcException(string key)
    {
        // Arrange
        var invalidRequest = new ProductRevisionRequest
        {
            Key = key,
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenRevisionNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _mocker.GetMock<IGetRevisionManager>()
            .Setup(x => x.GetRevisionAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse(ManagerResponseState.NotFound));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testRequest));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldThrowRpcException()
    {
        // Arrange
        _mocker.GetMock<IGetRevisionManager>()
            .Setup(x => x.GetRevisionAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}