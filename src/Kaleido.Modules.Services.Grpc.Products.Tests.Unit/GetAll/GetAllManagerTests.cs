using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetAll;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAll;

public class GetAllManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllManager _sut;
    private readonly List<ProductEntity> _productEntities;
    private readonly List<ProductPriceEntity> _productPriceEntities;
    private readonly List<Product> _expectedProducts;

    public GetAllManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetAllManager>>(NullLogger<GetAllManager>.Instance);

        _productEntities = new List<ProductEntity>
        {
            new ProductEntity { Key = Guid.NewGuid(), Name = "Product 1", CategoryKey = Guid.NewGuid() },
            new ProductEntity { Key = Guid.NewGuid(), Name = "Product 2", CategoryKey = Guid.NewGuid() }
        };

        _productPriceEntities = new List<ProductPriceEntity>
        {
            new ProductPriceEntity { ProductKey = _productEntities[0].Key, Price = 9.99f, CurrencyKey = Guid.NewGuid() },
            new ProductPriceEntity { ProductKey = _productEntities[1].Key, Price = 19.99f, CurrencyKey = Guid.NewGuid() }
        };

        _expectedProducts = new List<Product>
        {
            new Product { Key = _productEntities[0].Key.ToString(), Name = "Product 1", CategoryKey = _productEntities[0].CategoryKey.ToString() },
            new Product { Key = _productEntities[1].Key.ToString(), Name = "Product 2", CategoryKey = _productEntities[1].CategoryKey.ToString() }
        };

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productEntities);

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productPriceEntities);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()))
            .Returns<ProductEntity, IEnumerable<ProductPriceEntity>>((product, prices) =>
                new Product { Key = product.Key.ToString(), Name = product.Name, CategoryKey = product.CategoryKey.ToString() });

        _sut = _mocker.CreateInstance<GetAllManager>();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(_expectedProducts.Count, result.Count());
        Assert.Equal(_expectedProducts[0].Key, result.ElementAt(0).Key);
        Assert.Equal(_expectedProducts[0].Name, result.ElementAt(0).Name);
        Assert.Equal(_expectedProducts[1].Key, result.ElementAt(1).Key);
        Assert.Equal(_expectedProducts[1].Name, result.ElementAt(1).Name);

        _mocker.GetMock<IProductsRepository>().Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPricesRepository>().Verify(x => x.GetAllByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mocker.GetMock<IProductMapper>().Verify(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAllAsync_NoProducts_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductEntity>());

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductPriceEntity>());

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Empty(result);
        _mocker.GetMock<IProductsRepository>().Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPricesRepository>().Verify(x => x.GetAllByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mocker.GetMock<IProductMapper>().Verify(x => x.FromEntities(It.IsAny<ProductEntity>(), It.IsAny<IEnumerable<ProductPriceEntity>>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAsync());
    }
}
