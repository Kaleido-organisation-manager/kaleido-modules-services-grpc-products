using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.Update;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;
using static Kaleido.Grpc.Categories.GrpcCategories;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Update;

public class UpdateHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly UpdateHandler _sut;
    private readonly ProductActionRequest _testRequest;
    private readonly ManagerResponse _testManagerResponse;
    private readonly ProductResponse _testResponse;

    public UpdateHandlerTests()
    {
        _mocker = new AutoMocker();

        // Setup test data
        var productKey = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var currencyKey = Guid.NewGuid();

        _testRequest = new ProductActionRequest
        {
            Key = productKey.ToString(),
            Product = new Product
            {
                Name = "Updated Product",
                Description = "Updated Description",
                CategoryKey = Guid.NewGuid().ToString(),
                ImageUrl = "https://example.com/image.jpg",
                Prices =
                {
                    new ProductPrice
                    {
                        Units = 29,
                        Nanos = 99,
                        CurrencyKey = currencyKey.ToString()
                    }
                }
            }
        };

        var productEntity = new ProductEntity
        {
            Name = "Updated Product",
            Description = "Updated Description",
            CategoryKey = Guid.Parse(_testRequest.Product.CategoryKey),
            ImageUrl = "https://example.com/image.jpg"
        };

        var productRevision = new ProductRevisionEntity
        {
            Key = productKey,
            CreatedAt = timestamp,
            Action = RevisionAction.Updated
        };

        var productResult = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = productEntity,
            Revision = productRevision
        };

        var priceEntity = new ProductPriceEntity
        {
            Units = 29,
            Nanos = 99,
            CurrencyKey = currencyKey,
            ProductKey = productKey
        };

        var priceRevision = new ProductPriceRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = timestamp,
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
                CategoryKey = _testRequest.Product.CategoryKey,
                Prices =
                {
                    new ProductPriceResponse
                    {
                        Price = new ProductPrice
                        {
                            Units = priceEntity.Units,
                            Nanos = priceEntity.Nanos,
                            CurrencyKey = currencyKey.ToString()
                        }
                    }
                }
            },
            Revision = new BaseRevision
            {
                Key = productRevision.Key.ToString(),
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(productRevision.CreatedAt),
                Action = productRevision.Action.ToString()
            }
        };

        // Setup happy paths
        _mocker.GetMock<IUpdateManager>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductEntity>(),
                It.IsAny<IEnumerable<ProductPriceEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testManagerResponse);

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

        _mocker.Use(keyValidator);
        _mocker.Use(new ProductValidator(categoryKeyValidator, nameValidator, productPriceValidator));
        _mocker.Use(NullLogger<UpdateHandler>.Instance);

        _sut = _mocker.CreateInstance<UpdateHandler>();
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

        _mocker.GetMock<IUpdateManager>()
            .Verify(x => x.UpdateAsync(
                Guid.Parse(_testRequest.Key),
                It.IsAny<ProductEntity>(),
                It.IsAny<IEnumerable<ProductPriceEntity>>(),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    public async Task HandleAsync_WithInvalidKey_ShouldThrowRpcException(string key)
    {
        // Arrange
        var invalidRequest = new ProductActionRequest
        {
            Key = key,
            Product = _testRequest.Product
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
        // Assert.Equal("Invalid key.", exception.Status.Detail);
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidProduct_ShouldThrowRpcException()
    {
        // Arrange
        var invalidRequest = new ProductActionRequest
        {
            Key = Guid.NewGuid().ToString(),
            Product = new Product() // Empty product
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(invalidRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenProductNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _mocker.GetMock<IUpdateManager>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductEntity>(),
                It.IsAny<IEnumerable<ProductPriceEntity>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse(ManagerResponseState.NotFound));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testRequest));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldThrowRpcException()
    {
        // Arrange
        _mocker.GetMock<IUpdateManager>()
            .Setup(x => x.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<ProductEntity>(),
                It.IsAny<IEnumerable<ProductPriceEntity>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}