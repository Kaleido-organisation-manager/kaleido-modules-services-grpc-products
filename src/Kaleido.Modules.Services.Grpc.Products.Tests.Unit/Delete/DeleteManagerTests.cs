using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Delete;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Delete;

public class DeleteManagerTests
{
    private readonly AutoMocker _mocker;
    private readonly DeleteManager _sut;
    private readonly string _validProductKey;

    public DeleteManagerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<DeleteManager>>(NullLogger<DeleteManager>.Instance);
        _sut = _mocker.CreateInstance<DeleteManager>();

        _validProductKey = Guid.NewGuid().ToString();

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.DeleteByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task DeleteAsync_ValidKey_DeletesProductAndPrices()
    {
        // Act
        await _sut.DeleteAsync(_validProductKey);

        // Assert
        _mocker.GetMock<IProductsRepository>().Verify(
            x => x.DeleteAsync(It.Is<Guid>(g => g == Guid.Parse(_validProductKey)), It.IsAny<CancellationToken>()),
            Times.Once);
        _mocker.GetMock<IProductPricesRepository>().Verify(
            x => x.DeleteByProductKeyAsync(It.Is<Guid>(g => g == Guid.Parse(_validProductKey)), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_InvalidKey_ThrowsFormatException()
    {
        // Arrange
        var invalidKey = "invalid-guid";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => _sut.DeleteAsync(invalidKey));
    }

    [Fact]
    public async Task DeleteAsync_ProductRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new Exception("Product not found");

        _mocker.GetMock<IProductsRepository>()
            .Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(_validProductKey));
        Assert.Same(expectedException, exception);
    }

    [Fact]
    public async Task DeleteAsync_PriceRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var expectedException = new Exception("Error deleting prices");

        _mocker.GetMock<IProductPricesRepository>()
            .Setup(x => x.DeleteByProductKeyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(_validProductKey));
        Assert.Same(expectedException, exception);
    }
}
