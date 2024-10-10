using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Create;

public class CreateManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly CreateManager _sut;
    private readonly ProductEntity _expectedProductEntity;
    private readonly List<ProductPriceEntity> _expectedPriceEntities;

    public CreateManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<CreateManager>>(NullLogger<CreateManager>.Instance);

        // Happy path setup
        _expectedProductEntity = new ProductEntity { Name = "Sample Product", CategoryKey = Guid.NewGuid() };
        _expectedPriceEntities = new List<ProductPriceEntity>() {
            new ProductPriceEntity { CurrencyKey = Guid.NewGuid(), Price = 9.99f, ProductKey = _expectedProductEntity.Key }
        };

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.ToCreateEntity(It.IsAny<Product>(), It.IsAny<int>()))
            .Returns(_expectedProductEntity);

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProductEntity);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.ToCreatePriceEntity(It.IsAny<Guid>(), It.IsAny<ProductPrice>(), It.IsAny<int>()))
            .Returns(_expectedPriceEntities.First());

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.CreateRangeAsync(It.IsAny<IEnumerable<ProductPriceEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedPriceEntities);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()))
            .Returns(CreateSampleProduct());

        _sut = _mocker.CreateInstance<CreateManager>();
    }

    [Fact]
    public async Task CreateAsync_ValidProduct_ReturnsCreatedProduct()
    {
        // Arrange
        var createProduct = CreateSampleCreateProduct();
        var expectedProduct = CreateSampleProduct();

        // Act
        var result = await _sut.CreateAsync(createProduct);

        // Assert
        Assert.NotNull(result);
        _mocker.GetMock<IProductsRepository>().Verify(x => x.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPricesRepository>().Verify(x => x.CreateRangeAsync(It.IsAny<IEnumerable<ProductPriceEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyPrices_CreatesProductWithoutPrices()
    {
        // Arrange
        var createProduct = CreateSampleCreateProduct();
        createProduct.Prices.Clear();
        var expectedProduct = CreateSampleProduct();
        expectedProduct.Prices.Clear();

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()))
            .Returns(expectedProduct);

        // Act
        var result = await _sut.CreateAsync(createProduct);

        // Assert
        Assert.Equal(expectedProduct, result);
        _mocker.GetMock<IProductsRepository>().Verify(x => x.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPricesRepository>().Verify(x => x.CreateRangeAsync(It.IsAny<IEnumerable<ProductPriceEntity>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var createProduct = CreateSampleCreateProduct();
        var expectedException = new Exception("Database error");

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.CreateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(createProduct));
        Assert.Same(expectedException, exception);
    }

    private static CreateProduct CreateSampleCreateProduct()
    {
        return new CreateProduct
        {
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Sample Description",
            Name = "Sample Product",
            ImageUrl = "http://example.com/image.jpg",
            Prices = { new ProductPrice { CurrencyKey = "USD", Value = 9.99f } }
        };
    }

    private static Product CreateSampleProduct()
    {
        return new Product
        {
            Key = Guid.NewGuid().ToString(),
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Sample Description",
            Name = "Sample Product",
            ImageUrl = "http://example.com/image.jpg",
            Prices = { new ProductPrice { CurrencyKey = "USD", Value = 9.99f } }
        };
    }
}