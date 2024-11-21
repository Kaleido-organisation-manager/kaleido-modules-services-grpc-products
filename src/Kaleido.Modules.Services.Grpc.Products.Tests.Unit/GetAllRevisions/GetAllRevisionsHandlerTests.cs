using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.GetAllRevisions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllRevisions;

public class GetAllRevisionsHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllRevisionsHandler _sut;
    private readonly ProductRequest _testRequest;
    private readonly IEnumerable<ManagerResponse> _testManagerResponses;
    private readonly ProductListResponse _testResponse;

    public GetAllRevisionsHandlerTests()
    {
        _mocker = new AutoMocker();

        // Setup test data
        var productKey = Guid.NewGuid();
        _testRequest = new ProductRequest { Key = productKey.ToString() };

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
            CreatedAt = DateTime.UtcNow,
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
            CreatedAt = DateTime.UtcNow,
            Action = RevisionAction.Created
        };

        var priceResult = new EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>
        {
            Entity = priceEntity,
            Revision = priceRevision
        };

        _testManagerResponses = new[]
        {
            new ManagerResponse(productResult, new[] { priceResult })
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
                }
            }
        };

        // Setup happy paths
        _mocker.GetMock<IGetAllRevisionsManager>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testManagerResponses);

        _mocker.Use(new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>()).CreateMapper());
        _mocker.Use(new KeyValidator());
        _mocker.Use(NullLogger<GetAllRevisionsHandler>.Instance);

        _sut = _mocker.CreateInstance<GetAllRevisionsHandler>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldReturnResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_testRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Products);
        Assert.Equal(_testResponse.Products[0].Product.Name, result.Products[0].Product.Name);
        Assert.Equal(_testResponse.Products[0].Product.Description, result.Products[0].Product.Description);
        Assert.Equal(_testResponse.Products[0].Product.CategoryKey, result.Products[0].Product.CategoryKey);
        Assert.Single(result.Products[0].Product.Prices);
        Assert.Equal(_testResponse.Products[0].Product.Prices[0].Price.Units, result.Products[0].Product.Prices[0].Price.Units);
        Assert.Equal(_testResponse.Products[0].Product.Prices[0].Price.Nanos, result.Products[0].Product.Prices[0].Price.Nanos);

        _mocker.GetMock<IGetAllRevisionsManager>()
            .Verify(x => x.GetAllRevisionsAsync(Guid.Parse(_testRequest.Key), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    public async Task HandleAsync_WithInvalidKey_ShouldThrowRpcException(string key)
    {
        // Arrange
        var invalidRequest = new ProductRequest { Key = key };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldThrowRpcException()
    {
        // Arrange
        _mocker.GetMock<IGetAllRevisionsManager>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}