using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Get;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Get;

public class GetManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetManager _sut;
    private readonly string _validProductKey;
    private readonly ProductEntity _expectedProductEntity;
    private readonly List<ProductPriceEntity> _expectedPriceEntities;
    private readonly Product _expectedProduct;

    public GetManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetManager>>(NullLogger<GetManager>.Instance);
        _sut = _mocker.CreateInstance<GetManager>();

        _validProductKey = Guid.NewGuid().ToString();
        _expectedProductEntity = new ProductEntity { Key = Guid.Parse(_validProductKey), Name = "Sample Product", CategoryKey = Guid.NewGuid() };
        _expectedPriceEntities = new List<ProductPriceEntity>
        {
            new ProductPriceEntity { CurrencyKey = Guid.NewGuid(), Price = 9.99f, ProductKey = _expectedProductEntity.Key }
        };
        _expectedProduct = CreateSampleProduct(_validProductKey);

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetActiveAsync(Guid.Parse(_validProductKey), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProductEntity);

        _mocker.GetMock<IProductPriceRepository>()
            .Setup(x => x.GetAllActiveByProductKeyAsync(_expectedProductEntity.Key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedPriceEntities);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.FromEntities(_expectedProductEntity, _expectedPriceEntities))
            .Returns(_expectedProduct);
    }

    [Fact]
    public async Task GetAsync_ValidKey_ReturnsProduct()
    {
        // Act
        var result = await _sut.GetAsync(_validProductKey);

        // Assert
        Assert.Equal(_expectedProduct, result);
        _mocker.GetMock<IProductRepository>().Verify(x => x.GetActiveAsync(Guid.Parse(_validProductKey), It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductPriceRepository>().Verify(x => x.GetAllActiveByProductKeyAsync(_expectedProductEntity.Key, It.IsAny<CancellationToken>()), Times.Once);
        _mocker.GetMock<IProductMapper>().Verify(x => x.FromEntities(_expectedProductEntity, _expectedPriceEntities), Times.Once);
    }

    [Fact]
    public async Task GetAsync_InvalidKey_ThrowsFormatException()
    {
        // Arrange
        var invalidKey = "invalid-key";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAsync(invalidKey));
    }

    [Fact]
    public async Task GetAsync_ProductNotFound_ReturnsNull()
    {
        // Arrange
        var nonExistentKey = Guid.NewGuid().ToString();
        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetActiveAsync(Guid.Parse(nonExistentKey), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _sut.GetAsync(nonExistentKey);

        // Assert
        Assert.Null(result);
    }

    private static Product CreateSampleProduct(string key)
    {
        return new Product
        {
            Key = key,
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Sample Description",
            Name = "Sample Product",
            ImageUrl = "http://example.com/image.jpg",
            Prices = { new ProductPrice { CurrencyKey = "USD", Value = 9.99f } }
        };
    }
}
