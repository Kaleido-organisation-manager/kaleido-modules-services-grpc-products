using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Update;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Update;

public class UpdateManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly UpdateManager _sut;
    private readonly Product _validProduct;
    private readonly ProductEntity _storedProductEntity;
    private readonly List<ProductPriceEntity> _storedProductPrices;

    public UpdateManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<UpdateManager>>(NullLogger<UpdateManager>.Instance);
        _mocker.Use<IProductMapper>(new ProductMapper());

        var productKey = Guid.NewGuid();

        _validProduct = new Product
        {
            Key = productKey.ToString(),
            Name = "Updated Product",
            Description = "Updated Description",
            CategoryKey = Guid.NewGuid().ToString(),
            ImageUrl = "http://example.com/updated-image.jpg",
            Prices = { new ProductPrice { CurrencyKey = Guid.NewGuid().ToString(), Value = 19.99f } }
        };

        _storedProductEntity = new ProductEntity
        {
            Key = productKey,
            Name = "Original Product",
            Description = "Original Description",
            CategoryKey = Guid.Parse(_validProduct.CategoryKey),
            ImageUrl = "http://example.com/original-image.jpg",
            Revision = 1,
            CreatedAt = DateTime.UtcNow,
            Status = EntityStatus.Active
        };

        _storedProductPrices = new List<ProductPriceEntity>
        {
            new ProductPriceEntity
            {
                Key = Guid.NewGuid(),
                ProductKey = productKey,
                CurrencyKey = Guid.Parse(_validProduct.Prices[0].CurrencyKey),
                Price = 9.99f,
                Revision = 1,
                CreatedAt = DateTime.UtcNow,
                Status = EntityStatus.Active
            }
        };

        _sut = _mocker.CreateInstance<UpdateManager>();

        // Happy path setup
        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_storedProductEntity);

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity entity, CancellationToken _) => entity);

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.GetAllByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_storedProductPrices);

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.UpdateAsync(It.IsAny<ProductPriceEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductPriceEntity entity, CancellationToken _) => entity);
    }

    [Fact]
    public async Task UpdateAsync_ValidProduct_ReturnsUpdatedProduct()
    {
        // Act
        var result = await _sut.UpdateAsync(_validProduct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_validProduct.Key, result.Key);
        Assert.Equal(_validProduct.Name, result.Name);
        Assert.Equal(_validProduct.Description, result.Description);
        Assert.Equal(_validProduct.CategoryKey, result.CategoryKey);
        Assert.Equal(_validProduct.ImageUrl, result.ImageUrl);
        Assert.Single(result.Prices);
        Assert.Equal(_validProduct.Prices[0].CurrencyKey, result.Prices[0].CurrencyKey);
        Assert.Equal(_validProduct.Prices[0].Value, result.Prices[0].Value);
    }

    [Fact]
    public async Task UpdateAsync_ProductNotFound_ReturnsNull()
    {
        // Arrange
        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _sut.UpdateAsync(_validProduct);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ProductUnchanged_DoesNotUpdateProduct()
    {
        // Setup
        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductEntity()
            {
                Key = _storedProductEntity.Key,
                Name = _validProduct.Name,
                Description = _validProduct.Description,
                CategoryKey = Guid.Parse(_validProduct.CategoryKey),
                ImageUrl = _validProduct.ImageUrl,
                Revision = _storedProductEntity.Revision,
                CreatedAt = _storedProductEntity.CreatedAt,
                Status = _storedProductEntity.Status
            });

        // Act
        await _sut.UpdateAsync(_validProduct);

        // Assert
        _mocker.GetMock<IProductsRepository>()
            .Verify(x => x.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NewProductPrice_CreatesNewPrice()
    {
        // Arrange
        var newPrice = new ProductPrice { CurrencyKey = Guid.NewGuid().ToString(), Value = 29.99f };
        var productWithNewPrice = _validProduct.Clone();
        productWithNewPrice.Prices.Add(newPrice);

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.CreateAsync(It.IsAny<ProductPriceEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductPriceEntity
            {
                Key = Guid.NewGuid(),
                ProductKey = Guid.Parse(_validProduct.Key),
                CurrencyKey = Guid.Parse(newPrice.CurrencyKey),
                Price = newPrice.Value,
                Revision = 1,
                CreatedAt = DateTime.UtcNow,
                Status = EntityStatus.Active
            });

        // Act
        await _sut.UpdateAsync(productWithNewPrice);

        // Assert
        _mocker.GetMock<IProductPricesRepository>()
            .Verify(x => x.CreateAsync(It.IsAny<ProductPriceEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_RemovedProductPrice_ArchivesPrice()
    {
        // Arrange
        var productWithoutPrice = _validProduct.Clone();
        productWithoutPrice.Prices.Clear();

        // Act
        await _sut.UpdateAsync(productWithoutPrice);

        // Assert
        _mocker.GetMock<IProductPricesRepository>()
            .Verify(x => x.GetAllByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IProductPricesRepository>()
            .Verify(x => x.UpdateStatusAsync(It.IsAny<Guid>(), EntityStatus.Archived, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldHookOnToOldRevision_WhenCurrencyKeyIsReverted()
    {
        // Arrange
        var originalCurrencyKey = Guid.NewGuid();
        var newCurrencyKey = Guid.NewGuid();
        var productKey = _storedProductEntity.Key;

        var originalProductPriceKey = Guid.NewGuid();
        var originalProductPrice = new ProductPriceEntity
        {
            Key = originalProductPriceKey,
            ProductKey = productKey,
            CurrencyKey = originalCurrencyKey,
            Price = 9.99f,
            Revision = 1,
            CreatedAt = DateTime.UtcNow,
            Status = EntityStatus.Archived
        };

        var updatedProductPriceKey = Guid.NewGuid();
        var updatedProductPrice = new ProductPriceEntity
        {
            Key = updatedProductPriceKey,
            ProductKey = productKey,
            CurrencyKey = newCurrencyKey,
            Price = 19.99f,
            Revision = 1,
            CreatedAt = DateTime.UtcNow,
            Status = EntityStatus.Active
        };

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.GetAllByProductKeyAsync(productKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductPriceEntity> { originalProductPrice, updatedProductPrice });

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.UpdateAsync(It.IsAny<ProductPriceEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductPriceEntity entity, CancellationToken _) => entity);

        var revertedProduct = _validProduct.Clone();
        revertedProduct.Key = _storedProductEntity.Key.ToString();
        revertedProduct.Prices.Clear();
        revertedProduct.Prices.Add(new ProductPrice { CurrencyKey = originalCurrencyKey.ToString(), Value = 9.99f });

        // Act
        var result = await _sut.UpdateAsync(revertedProduct);

        // Assert
        Assert.NotNull(result);
        _mocker.GetMock<IProductPricesRepository>()
            .Verify(x => x.UpdateAsync(It.Is<ProductPriceEntity>(e =>
                e.CurrencyKey == originalCurrencyKey &&
                e.Status == EntityStatus.Active &&
                e.Revision == 2), It.IsAny<CancellationToken>()), Times.Once);

        _mocker.GetMock<IProductPricesRepository>()
            .Verify(x => x.UpdateStatusAsync(updatedProductPriceKey, EntityStatus.Archived, It.IsAny<CancellationToken>()), Times.Once);
    }
}
