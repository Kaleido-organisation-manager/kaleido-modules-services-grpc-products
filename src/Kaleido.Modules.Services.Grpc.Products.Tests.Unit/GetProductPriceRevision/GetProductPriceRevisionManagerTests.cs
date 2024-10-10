using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevision;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductPriceRevision;

public class GetProductPriceRevisionManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetProductPriceRevisionManager _sut;
    private readonly ProductPriceRevision _expectedRevision;
    private readonly ProductPriceEntity _productPriceRevisionEntity;

    public GetProductPriceRevisionManagerTests()
    {
        _mocker = new AutoMocker();

        var productKey = Guid.NewGuid();
        var currencyKey = Guid.NewGuid();

        _expectedRevision = new ProductPriceRevision
        {
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Value = 9.99f,
            CurrencyKey = currencyKey.ToString(),
            Revision = 1,
            Status = "Active"
        };

        _productPriceRevisionEntity = new ProductPriceEntity
        {
            Key = Guid.NewGuid(),
            ProductKey = productKey,
            CurrencyKey = currencyKey,
            Revision = 1,
            Price = 9.99f
        };

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.GetRevisionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productPriceRevisionEntity);

        _mocker.Use<ILogger<GetProductPriceRevisionManager>>(NullLogger<GetProductPriceRevisionManager>.Instance);
        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.ToProductPriceRevision(It.IsAny<ProductPriceEntity>()))
            .Returns(_expectedRevision);

        _sut = _mocker.CreateInstance<GetProductPriceRevisionManager>();
    }

    [Fact]
    public async Task GetAsync_ValidRequest_ReturnsProductPriceRevision()
    {
        // Arrange
        var productKey = Guid.NewGuid().ToString();
        var currencyKey = Guid.NewGuid().ToString();

        // Act
        var result = await _sut.GetAsync(productKey, currencyKey, _expectedRevision.Revision);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_expectedRevision, result);

        _mocker.GetMock<IProductPricesRepository>().Verify(
            x => x.GetRevisionAsync(Guid.Parse(productKey), Guid.Parse(currencyKey), _expectedRevision.Revision, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_InvalidProductKey_ThrowsFormatException()
    {
        // Arrange
        var invalidProductKey = "invalid-guid";
        var currencyKey = Guid.NewGuid().ToString();
        var expectedRevision = 1;

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAsync(invalidProductKey, currencyKey, expectedRevision));
    }

    [Fact]
    public async Task GetAsync_InvalidCurrencyKey_ThrowsFormatException()
    {
        // Arrange
        var productKey = Guid.NewGuid().ToString();
        var invalidCurrencyKey = "invalid-guid";
        var expectedRevision = 1;

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAsync(productKey, invalidCurrencyKey, expectedRevision));
    }

    [Fact]
    public async Task GetAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var productKey = Guid.NewGuid().ToString();
        var currencyKey = Guid.NewGuid().ToString();
        var expectedRevision = 1;
        var expectedException = new Exception("Database error");

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.GetRevisionAsync(Guid.Parse(productKey), Guid.Parse(currencyKey), expectedRevision, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAsync(productKey, currencyKey, expectedRevision));
        Assert.Same(expectedException, exception);
    }
}
