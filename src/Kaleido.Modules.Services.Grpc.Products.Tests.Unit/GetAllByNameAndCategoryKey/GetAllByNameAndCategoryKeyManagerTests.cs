using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetAllByNameAndCategoryKey;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllByNameAndCategoryKey;

public class GetAllByNameAndCategoryKeyManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllByNameAndCategoryKeyManager _sut;
    private readonly string _validName;
    private readonly string _validCategoryKey;
    private readonly List<ProductEntity> _productEntities;
    private readonly List<ProductPriceEntity> _productPriceEntities;
    private readonly List<Product> _expectedProducts;

    public GetAllByNameAndCategoryKeyManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetAllByNameAndCategoryKeyManager>>(NullLogger<GetAllByNameAndCategoryKeyManager>.Instance);

        _validName = "Test Product";
        _validCategoryKey = Guid.NewGuid().ToString();
        _productEntities = new List<ProductEntity>
        {
            new ProductEntity { Key = Guid.NewGuid(), Name = "Test Product 1", CategoryKey = Guid.Parse(_validCategoryKey) },
            new ProductEntity { Key = Guid.NewGuid(), Name = "Test Product 2", CategoryKey = Guid.Parse(_validCategoryKey) }
        };

        _productPriceEntities = new List<ProductPriceEntity>
        {
            new ProductPriceEntity { ProductKey = _productEntities[0].Key, Price = 9.99f, CurrencyKey = Guid.NewGuid() },
            new ProductPriceEntity { ProductKey = _productEntities[1].Key, Price = 19.99f, CurrencyKey = Guid.NewGuid() }
        };

        _expectedProducts = new List<Product>
        {
            new Product { Key = _productEntities[0].Key.ToString(), Name = "Test Product 1", CategoryKey = _validCategoryKey },
            new Product { Key = _productEntities[1].Key.ToString(), Name = "Test Product 2", CategoryKey = _validCategoryKey }
        };

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetAllByNameAndCategoryKeyAsync(_validName, Guid.Parse(_validCategoryKey), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productEntities);

        _mocker.GetMock<IProductPriceRepository>()
            .Setup(x => x.GetAllActiveByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productPriceEntities);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()))
            .Returns<ProductEntity, IEnumerable<ProductPriceEntity>>((product, prices) =>
                new Product { Key = product.Key.ToString(), Name = product.Name, CategoryKey = product.CategoryKey.ToString() });

        _sut = _mocker.CreateInstance<GetAllByNameAndCategoryKeyManager>();
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKeyAsync_ValidNameAndCategoryKey_ReturnsProducts()
    {
        // Act
        var result = await _sut.GetAllByNameAndCategoryKeyAsync(_validName, _validCategoryKey);

        // Assert
        Assert.Equal(_expectedProducts.Count, result.Count());
        Assert.Equal(_expectedProducts[0].Key, result.ElementAt(0).Key);
        Assert.Equal(_expectedProducts[0].Name, result.ElementAt(0).Name);
        Assert.Equal(_expectedProducts[1].Key, result.ElementAt(1).Key);
        Assert.Equal(_expectedProducts[1].Name, result.ElementAt(1).Name);

        _mocker.GetMock<IProductRepository>().Verify(x => x.GetAllByNameAndCategoryKeyAsync(_validName, Guid.Parse(_validCategoryKey), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPriceRepository>().Verify(x => x.GetAllActiveByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mocker.GetMock<IProductMapper>().Verify(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKeyAsync_InvalidCategoryKey_ThrowsFormatException()
    {
        // Arrange
        var invalidCategoryKey = "invalid-guid";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAllByNameAndCategoryKeyAsync(_validName, invalidCategoryKey));
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKeyAsync_NoProducts_ReturnsEmptyList()
    {
        // Arrange
        var name = "Non-existent Product";
        var categoryKey = Guid.NewGuid().ToString();

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetAllByNameAndCategoryKeyAsync(name, Guid.Parse(categoryKey), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductEntity>());

        // Act
        var result = await _sut.GetAllByNameAndCategoryKeyAsync(name, categoryKey);

        // Assert
        Assert.Empty(result);
        _mocker.GetMock<IProductRepository>().Verify(x => x.GetAllByNameAndCategoryKeyAsync(name, Guid.Parse(categoryKey), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPriceRepository>().Verify(x => x.GetAllActiveByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mocker.GetMock<IProductMapper>().Verify(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()), Times.Never);
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKeyAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new Exception("Database error");

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetAllByNameAndCategoryKeyAsync(_validName, Guid.Parse(_validCategoryKey), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllByNameAndCategoryKeyAsync(_validName, _validCategoryKey));
        Assert.Same(expectedException, exception);
    }
}

