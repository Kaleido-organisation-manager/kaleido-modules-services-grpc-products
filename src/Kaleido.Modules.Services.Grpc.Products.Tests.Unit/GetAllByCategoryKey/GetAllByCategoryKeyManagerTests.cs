using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllByCategoryKey;

public class GetAllByCategoryKeyManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllByCategoryKeyManager _sut;
    private readonly string _validCategoryKey;
    private readonly List<ProductEntity> _productEntities;
    private readonly List<ProductPriceEntity> _productPriceEntities;
    private readonly List<Product> _expectedProducts;

    public GetAllByCategoryKeyManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetAllByCategoryKeyManager>>(NullLogger<GetAllByCategoryKeyManager>.Instance);

        _validCategoryKey = Guid.NewGuid().ToString();
        _productEntities = new List<ProductEntity>
        {
            new ProductEntity { Key = Guid.NewGuid(), Name = "Product 1", CategoryKey = Guid.Parse(_validCategoryKey) },
            new ProductEntity { Key = Guid.NewGuid(), Name = "Product 2", CategoryKey = Guid.Parse(_validCategoryKey) }
        };

        _productPriceEntities = new List<ProductPriceEntity>
        {
            new ProductPriceEntity { ProductKey = _productEntities[0].Key, Price = 9.99f, CurrencyKey = Guid.NewGuid() },
            new ProductPriceEntity { ProductKey = _productEntities[1].Key, Price = 19.99f, CurrencyKey = Guid.NewGuid() }
        };

        _expectedProducts = new List<Product>
        {
            new Product { Key = _productEntities[0].Key.ToString(), Name = "Product 1", CategoryKey = _validCategoryKey },
            new Product { Key = _productEntities[1].Key.ToString(), Name = "Product 2", CategoryKey = _validCategoryKey }
        };

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetAllByCategoryIdAsync(Guid.Parse(_validCategoryKey), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productEntities);

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.GetAllByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productPriceEntities);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()))
            .Returns<ProductEntity, IEnumerable<ProductPriceEntity>>((product, prices) =>
                new Product { Key = product.Key.ToString(), Name = product.Name, CategoryKey = product.CategoryKey.ToString() });

        _sut = _mocker.CreateInstance<GetAllByCategoryKeyManager>();
    }

    [Fact]
    public async Task GetAllAsync_ValidCategoryKey_ReturnsProducts()
    {
        // Act
        var result = await _sut.GetAllAsync(_validCategoryKey);

        // Assert
        Assert.Equal(_expectedProducts.Count, result.Count());
        Assert.Equal(_expectedProducts[0].Key, result.ElementAt(0).Key);
        Assert.Equal(_expectedProducts[0].Name, result.ElementAt(0).Name);
        Assert.Equal(_expectedProducts[1].Key, result.ElementAt(1).Key);
        Assert.Equal(_expectedProducts[1].Name, result.ElementAt(1).Name);

        _mocker.GetMock<IProductsRepository>().Verify(x => x.GetAllByCategoryIdAsync(Guid.Parse(_validCategoryKey), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPricesRepository>().Verify(x => x.GetAllByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mocker.GetMock<IProductMapper>().Verify(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAllAsync_InvalidCategoryKey_ThrowsFormatException()
    {
        // Arrange
        var invalidCategoryKey = "invalid-guid";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAllAsync(invalidCategoryKey));
    }

    [Fact]
    public async Task GetAllAsync_NoProducts_ReturnsEmptyList()
    {
        // Arrange
        var categoryKey = Guid.NewGuid().ToString();

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetAllByCategoryIdAsync(Guid.Parse(categoryKey), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductEntity>());

        // Act
        var result = await _sut.GetAllAsync(categoryKey);

        // Assert
        Assert.Empty(result);
        _mocker.GetMock<IProductsRepository>().Verify(x => x.GetAllByCategoryIdAsync(Guid.Parse(categoryKey), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPricesRepository>().Verify(x => x.GetAllByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mocker.GetMock<IProductMapper>().Verify(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var categoryKey = Guid.NewGuid().ToString();
        var expectedException = new Exception("Database error");

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetAllByCategoryIdAsync(Guid.Parse(categoryKey), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAsync(categoryKey));
        Assert.Same(expectedException, exception);
    }
}
