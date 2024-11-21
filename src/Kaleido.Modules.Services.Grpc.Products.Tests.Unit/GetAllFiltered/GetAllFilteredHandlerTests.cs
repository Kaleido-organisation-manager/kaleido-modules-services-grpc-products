using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.GetAllFiltered;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;
using static Kaleido.Grpc.Categories.GrpcCategories;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllFiltered;

public class GetAllFilteredHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllFilteredHandler _sut;
    private readonly ProductFilterRequest _testRequest;
    private readonly List<ManagerResponse> _testManagerResponses;
    private readonly ProductListResponse _testResponse;

    public GetAllFilteredHandlerTests()
    {
        _mocker = new AutoMocker();

        // Setup test data
        _testRequest = new ProductFilterRequest
        {
            Name = "Test",
            CategoryKey = Guid.NewGuid().ToString()
        };

        var productEntity = new ProductEntity
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.Parse(_testRequest.CategoryKey),
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
            Units = 9,
            Nanos = 99,
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
        _mocker.GetMock<IGetAllFilteredManager>()
            .Setup(x => x.GetAllFilteredAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testManagerResponses);

        _mocker.Use(new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>()).CreateMapper());

        var keyValidator = new KeyValidator();
        var nameValidator = new NameValidator();
        var currencyKeyValidator = new CurrencyKeyValidator(keyValidator);
        var productPriceValidator = new ProductPriceValidator(currencyKeyValidator);

        // Mock only the external dependency (categories client)
        var categoriesClientMock = new Mock<GrpcCategoriesClient>();
        // // Setup happy path for category validation
        categoriesClientMock.Setup(c => c.GetCategoryAsync(It.IsAny<CategoryRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(new AsyncUnaryCall<CategoryResponse>(Task.FromResult(new CategoryResponse()), Task.FromResult(new Metadata()), null!, null!, null!));

        var categoryKeyValidator = new CategoryKeyValidator(keyValidator, categoriesClientMock.Object, NullLogger<CategoryKeyValidator>.Instance);

        _mocker.Use(categoryKeyValidator);
        _mocker.Use(new NameValidator());
        _mocker.Use(NullLogger<GetAllFilteredHandler>.Instance);

        _sut = _mocker.CreateInstance<GetAllFilteredHandler>();
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

        _mocker.GetMock<IGetAllFilteredManager>()
            .Verify(x => x.GetAllFilteredAsync(
                _testRequest.Name,
                _testRequest.CategoryKey,
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("invalid-guid")]
    public async Task HandleAsync_WithInvalidCategoryKey_ShouldThrowValidationException(string categoryKey)
    {
        // Arrange
        var invalidRequest = new ProductFilterRequest
        {
            Name = "Test",
            CategoryKey = categoryKey
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WithTooLongName_ShouldThrowValidationException()
    {
        // Arrange
        var invalidRequest = new ProductFilterRequest
        {
            Name = new string('a', 101),
            CategoryKey = Guid.NewGuid().ToString()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyFilters_ShouldNotValidate()
    {
        // Arrange
        var request = new ProductFilterRequest();

        // Act
        var result = await _sut.HandleAsync(request);

        // Assert
        Assert.NotNull(result);
        _mocker.GetMock<IGetAllFilteredManager>()
            .Verify(x => x.GetAllFilteredAsync(
                string.Empty,
                string.Empty,
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyResults_ShouldReturnEmptyResponse()
    {
        // Arrange
        _mocker.GetMock<IGetAllFilteredManager>()
            .Setup(x => x.GetAllFilteredAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ManagerResponse>());

        // Act
        var result = await _sut.HandleAsync(_testRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Products);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldThrowRpcException()
    {
        // Arrange
        _mocker.GetMock<IGetAllFilteredManager>()
            .Setup(x => x.GetAllFilteredAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            () => _sut.HandleAsync(_testRequest));

        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}