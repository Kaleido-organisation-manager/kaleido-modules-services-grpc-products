using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.GetAll;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAll;

public class GetAllHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllHandler _sut;
    private readonly EmptyRequest _testRequest;
    private readonly List<ManagerResponse> _testManagerResponses;
    private readonly ProductListResponse _testResponse;

    public GetAllHandlerTests()
    {
        _mocker = new AutoMocker();

        // Setup test data
        _testRequest = new EmptyRequest();

        var productEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            ImageUrl = "https://example.com/image.jpg"
        };

        var productRevision = new ProductRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var productResult = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = productEntity,
            Revision = productRevision
        };

        var priceEntity = new ProductPriceEntity
        {
            Value = 9.99f,
            CurrencyKey = Guid.NewGuid(),
            ProductKey = productRevision.Key
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

        _testManagerResponses = new List<ManagerResponse>
        {
            new(productResult, new[] { priceResult })
        };

        _testResponse = new ProductListResponse
        {
            Products =
            {
                new ProductResponse
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
                                    Value = priceEntity.Value,
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
                }
            }
        };

        // Setup happy paths
        _mocker.GetMock<IGetAllManager>()
            .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testManagerResponses);

        _mocker.Use(new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>()).CreateMapper());
        _mocker.Use(NullLogger<GetAllHandler>.Instance);

        _sut = _mocker.CreateInstance<GetAllHandler>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldReturnResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_testRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Products);
        Assert.Equal(_testResponse.Products[0].Product.Name, result.Products[0].Product.Name);
        Assert.Equal(_testResponse.Products[0].Product.Description, result.Products[0].Product.Description);
        Assert.Equal(_testResponse.Products[0].Product.CategoryKey, result.Products[0].Product.CategoryKey);
        Assert.Single(result.Products[0].Product.Prices);
        Assert.Equal(_testResponse.Products[0].Product.Prices[0].Price.Value, result.Products[0].Product.Prices[0].Price.Value);

        _mocker.GetMock<IGetAllManager>()
            .Verify(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyList_ShouldReturnEmptyResponse()
    {
        // Arrange
        _mocker.GetMock<IGetAllManager>()
            .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ManagerResponse>());

        // Act
        var result = await _sut.HandleAsync(_testRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Products);

        _mocker.GetMock<IGetAllManager>()
            .Verify(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldThrowRpcException()
    {
        // Arrange
        _mocker.GetMock<IGetAllManager>()
            .Setup(x => x.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_testRequest, CancellationToken.None));

        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}
