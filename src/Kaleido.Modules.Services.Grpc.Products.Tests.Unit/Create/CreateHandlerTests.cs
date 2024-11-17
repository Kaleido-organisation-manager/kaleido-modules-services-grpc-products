using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;
using static Kaleido.Grpc.Categories.GrpcCategories;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Create;

public class CreateHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly CreateHandler _sut;
    private readonly Product _testProduct;
    private readonly ProductEntity _testProductEntity;
    private readonly ProductResponse _testResponse;
    private readonly ManagerResponse _testManagerResponse;

    public CreateHandlerTests()
    {
        _mocker = new AutoMocker();

        // Setup test data
        _testProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Test Description",
            Prices = { new ProductPrice { Value = 10.0f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        _testProductEntity = new ProductEntity
        {
            Name = _testProduct.Name,
            CategoryKey = Guid.Parse(_testProduct.CategoryKey),
            Description = _testProduct.Description
        };

        var revision = new ProductRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var productResult = new EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>
        {
            Entity = _testProductEntity,
            Revision = revision
        };

        var priceEntity = new ProductPriceEntity
        {
            Value = _testProduct.Prices[0].Value,
            CurrencyKey = Guid.Parse(_testProduct.Prices[0].CurrencyKey),
            ProductKey = revision.Key
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

        _testResponse = new ProductResponse
        {
            Product = new ProductWithPricesResponse
            {
                Name = _testProduct.Name,
                Description = _testProduct.Description,
                CategoryKey = _testProduct.CategoryKey
            },
            Revision = new BaseRevision
            {
                Key = revision.Key.ToString(),
                CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(revision.CreatedAt)
            }
        };

        // Setup happy paths
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

        _mocker.Use(new ProductValidator(categoryKeyValidator, nameValidator, productPriceValidator));

        _mocker.GetMock<ICreateManager>()
            .Setup(x => x.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPrice>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testManagerResponse);

        _mocker.GetMock<IMapper>()
            .Setup(x => x.Map<ProductEntity>(_testProduct))
            .Returns(_testProductEntity);

        _mocker.GetMock<IMapper>()
            .Setup(x => x.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(_testManagerResponse.Product))
            .Returns(new EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>
            {
                Entity = new ProductWithPrices
                {
                    Name = _testProductEntity.Name,
                    Description = _testProductEntity.Description,
                    CategoryKey = _testProductEntity.CategoryKey,
                    Prices = _testManagerResponse.ProductPrices ?? []
                },
                Revision = _testManagerResponse.Product?.Revision ?? new BaseRevisionEntity()
            });

        _mocker.GetMock<IMapper>()
            .Setup(x => x.Map<ProductResponse>(It.IsAny<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>()))
            .Returns(_testResponse);

        _sut = _mocker.CreateInstance<CreateHandler>();
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldReturnResponse()
    {
        // Act
        var result = await _sut.HandleAsync(_testProduct);

        // Assert
        Assert.Equal(_testResponse, result);
        _mocker.GetMock<ICreateManager>()
            .Verify(x => x.CreateAsync(_testProductEntity, _testProduct.Prices, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidationError_ShouldThrowRpcException()
    {
        // Arrange
        _testProduct.Name = string.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testProduct));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_WithUnexpectedError_ShouldThrowRpcException()
    {
        // Arrange
        _mocker.GetMock<ICreateManager>()
            .Setup(x => x.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPrice>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _sut.HandleAsync(_testProduct));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}