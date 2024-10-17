using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductPriceRevisions;

public class GetProductPriceRevisionsManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetProductPriceRevisionsManager _sut;
    private readonly string _validProductKey;
    private readonly string _validCurrencyKey;
    private readonly List<ProductPriceEntity> _productPriceEntities;
    private readonly List<ProductPriceRevision> _expectedRevisions;

    public GetProductPriceRevisionsManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetProductPriceRevisionsManager>>(NullLogger<GetProductPriceRevisionsManager>.Instance);

        _validProductKey = Guid.NewGuid().ToString();
        _validCurrencyKey = Guid.NewGuid().ToString();
        _productPriceEntities = new List<ProductPriceEntity>
        {
            new ProductPriceEntity { Key = Guid.NewGuid(), ProductKey = Guid.Parse(_validProductKey), CurrencyKey = Guid.Parse(_validCurrencyKey), Revision = 1, Price = 9.99f },
            new ProductPriceEntity { Key = Guid.NewGuid(), ProductKey = Guid.Parse(_validProductKey), CurrencyKey = Guid.Parse(_validCurrencyKey), Revision = 2, Price = 10.99f }
        };
        _expectedRevisions = new List<ProductPriceRevision>
        {
            new ProductPriceRevision { Revision = 1, Value = 9.99f },
            new ProductPriceRevision { Revision = 2, Value = 10.99f }
        };

        _mocker.GetMock<IProductPriceRepository>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productPriceEntities);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.ToProductPriceRevision(It.IsAny<ProductPriceEntity>()))
            .Returns<ProductPriceEntity>(entity => new ProductPriceRevision { Revision = entity.Revision, Value = entity.Price });

        _sut = _mocker.CreateInstance<GetProductPriceRevisionsManager>();
    }

    [Fact]
    public async Task GetAllAsync_ValidRequest_ReturnsProductPriceRevisions()
    {
        // Act
        var result = await _sut.GetAllAsync(_validProductKey, _validCurrencyKey);

        // Assert
        Assert.Equal(_expectedRevisions.Count, result.Count());
        Assert.Equal(_expectedRevisions[0].Revision, result.ElementAt(0).Revision);
        Assert.Equal(_expectedRevisions[0].Value, result.ElementAt(0).Value);
        Assert.Equal(_expectedRevisions[1].Revision, result.ElementAt(1).Revision);
        Assert.Equal(_expectedRevisions[1].Value, result.ElementAt(1).Value);

        _mocker.GetMock<IProductPriceRepository>().Verify(
            x => x.GetAllRevisionsAsync(Guid.Parse(_validProductKey), Guid.Parse(_validCurrencyKey), It.IsAny<CancellationToken>()),
            Times.Once);
        _mocker.GetMock<IProductMapper>().Verify(x => x.ToProductPriceRevision(It.IsAny<ProductPriceEntity>()), Times.Exactly(_productPriceEntities.Count));
    }

    [Fact]
    public async Task GetAllAsync_NoRevisions_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IProductPriceRepository>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductPriceEntity>());

        // Act
        var result = await _sut.GetAllAsync(_validProductKey, _validCurrencyKey);

        // Assert
        Assert.Empty(result);
        _mocker.GetMock<IProductPriceRepository>().Verify(
            x => x.GetAllRevisionsAsync(Guid.Parse(_validProductKey), Guid.Parse(_validCurrencyKey), It.IsAny<CancellationToken>()),
            Times.Once);
        _mocker.GetMock<IProductMapper>().Verify(x => x.ToProductPriceRevision(It.IsAny<ProductPriceEntity>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_InvalidProductKey_ThrowsFormatException()
    {
        // Arrange
        var invalidProductKey = "invalid-guid";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAllAsync(invalidProductKey, _validCurrencyKey));
    }

    [Fact]
    public async Task GetAllAsync_InvalidCurrencyKey_ThrowsFormatException()
    {
        // Arrange
        var invalidCurrencyKey = "invalid-guid";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAllAsync(_validProductKey, invalidCurrencyKey));
    }

    [Fact]
    public async Task GetAllAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new Exception("Database error");

        _mocker.GetMock<IProductPriceRepository>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAsync(_validProductKey, _validCurrencyKey));
        Assert.Same(expectedException, exception);
    }
}