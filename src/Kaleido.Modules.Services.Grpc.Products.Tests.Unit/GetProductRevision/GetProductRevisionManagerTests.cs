using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetProductRevision;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductRevision;

public class GetProductRevisionManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetProductRevisionManager _sut;
    private readonly string _validProductKey;
    private readonly ProductEntity _expectedProductEntity;
    private readonly ProductRevision _expectedProductRevision;

    public GetProductRevisionManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetProductRevisionManager>>(NullLogger<GetProductRevisionManager>.Instance);
        _sut = _mocker.CreateInstance<GetProductRevisionManager>();

        _validProductKey = Guid.NewGuid().ToString();
        var revision = 1;
        _expectedProductEntity = new ProductEntity
        {
            Key = Guid.Parse(_validProductKey),
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid(),
            Revision = revision
        };
        _expectedProductRevision = new ProductRevision
        {
            Key = _validProductKey,
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = _expectedProductEntity.CategoryKey.ToString(),
            Revision = revision
        };

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetRevisionAsync(Guid.Parse(_validProductKey), revision, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProductEntity);

        _mocker.GetMock<IProductMapper>()
            .Setup(x => x.ToProductRevision(_expectedProductEntity))
            .Returns(_expectedProductRevision);
    }

    [Fact]
    public async Task GetAsync_ValidKeyAndRevision_ReturnsProductRevision()
    {
        // Act
        var result = await _sut.GetAsync(_validProductKey, _expectedProductRevision.Revision);

        // Assert
        Assert.Equal(_expectedProductRevision, result);
        _mocker.GetMock<IProductRepository>().Verify(
            x => x.GetRevisionAsync(Guid.Parse(_validProductKey), _expectedProductRevision.Revision, It.IsAny<CancellationToken>()),
            Times.Once);
        _mocker.GetMock<IProductMapper>().Verify(
            x => x.ToProductRevision(_expectedProductEntity),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_InvalidKey_ThrowsFormatException()
    {
        // Arrange
        var invalidKey = "invalid-guid";
        var revision = 1;

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.GetAsync(invalidKey, revision));
    }

    [Fact]
    public async Task GetAsync_RepositoryReturnsNull_ReturnsNull()
    {
        // Arrange
        var nonExistentKey = Guid.NewGuid().ToString();
        var revision = 1;
        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetRevisionAsync(Guid.Parse(nonExistentKey), revision, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _sut.GetAsync(nonExistentKey, revision);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var revision = 1;
        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetRevisionAsync(Guid.Parse(_validProductKey), revision, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.GetAsync(_validProductKey, revision));
    }
}
