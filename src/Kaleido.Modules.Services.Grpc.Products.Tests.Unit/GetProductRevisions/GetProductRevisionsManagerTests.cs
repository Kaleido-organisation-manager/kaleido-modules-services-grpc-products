using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductRevisions;

public class GetProductRevisionsManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetProductRevisionsManager _sut;
    private readonly string _validProductKey;
    private readonly List<ProductEntity> _productEntities;
    private readonly List<ProductRevision> _expectedRevisions;

    public GetProductRevisionsManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetProductRevisionsManager>>(NullLogger<GetProductRevisionsManager>.Instance);
        _sut = _mocker.CreateInstance<GetProductRevisionsManager>();

        _validProductKey = Guid.NewGuid().ToString();
        _productEntities = new List<ProductEntity>
        {
            new ProductEntity { Key = Guid.Parse(_validProductKey), Name = "Product 1", Revision = 1, CategoryKey = Guid.NewGuid() },
            new ProductEntity { Key = Guid.Parse(_validProductKey), Name = "Product 2", Revision = 2, CategoryKey = Guid.NewGuid() }
        };
        _expectedRevisions = new List<ProductRevision>
        {
            new ProductRevision { Revision = 1, Name = "Product 1" },
            new ProductRevision { Revision = 2, Name = "Product 2" }
        };

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_productEntities);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.ToProductRevision(It.IsAny<ProductEntity>()))
            .Returns<ProductEntity>(entity => new ProductRevision { Revision = entity.Revision, Name = entity.Name });
    }

    [Fact]
    public async Task GetAllAsync_ValidRequest_ReturnsProductRevisions()
    {
        // Act
        var result = await _sut.GetAllAsync(_validProductKey);

        // Assert
        Assert.Equal(_expectedRevisions.Count, result.Count());
        Assert.Equal(_expectedRevisions[0].Revision, result.ElementAt(0).Revision);
        Assert.Equal(_expectedRevisions[0].Name, result.ElementAt(0).Name);
        Assert.Equal(_expectedRevisions[1].Revision, result.ElementAt(1).Revision);
        Assert.Equal(_expectedRevisions[1].Name, result.ElementAt(1).Name);

        _mocker.GetMock<IProductRepository>().Verify(
            x => x.GetAllRevisionsAsync(Guid.Parse(_validProductKey), It.IsAny<CancellationToken>()),
            Times.Once);
        _mocker.GetMock<IProductMapper>().Verify(x => x.ToProductRevision(It.IsAny<ProductEntity>()), Times.Exactly(_productEntities.Count));
    }

    [Fact]
    public async Task GetAllAsync_NoRevisions_ReturnsEmptyList()
    {
        // Arrange
        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductEntity>());

        // Act
        var result = await _sut.GetAllAsync(_validProductKey);

        // Assert
        Assert.Empty(result);
        _mocker.GetMock<IProductRepository>().Verify(
            x => x.GetAllRevisionsAsync(Guid.Parse(_validProductKey), It.IsAny<CancellationToken>()),
            Times.Once);
        _mocker.GetMock<IProductMapper>().Verify(x => x.ToProductRevision(It.IsAny<ProductEntity>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_InvalidProductKey_ThrowsFormatException()
    {
        // Arrange
        var invalidProductKey = "invalid-guid";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAllAsync(invalidProductKey));
    }

    [Fact]
    public async Task GetAllAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new Exception("Database error");

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAsync(_validProductKey));
        Assert.Same(expectedException, exception);
    }
}
