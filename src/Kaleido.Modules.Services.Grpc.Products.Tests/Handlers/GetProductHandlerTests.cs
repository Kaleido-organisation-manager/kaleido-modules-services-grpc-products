using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Handlers;

[TestClass]
public class GetProductHandlerTests
{
    private IGetProductHandler _getProductHandler = null!;
    private Mock<IProductsManager> _productsManager = null!;
    private Mock<ILogger<GetProductHandler>> _logger = null!;
    private readonly AutoMocker _mocker = new AutoMocker();

    [TestInitialize]
    public void Initialize()
    {
        _logger = _mocker.GetMock<ILogger<GetProductHandler>>();
        _productsManager = _mocker.GetMock<IProductsManager>();
        _getProductHandler = _mocker.CreateInstance<GetProductHandler>();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldReturnProduct_WhenSuccessful()
    {
        // Arrange
        var productId = "test-product-id";
        var expectedProduct = new Product { /* initialize properties */ };

        _productsManager.Setup(pm => pm.GetProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var response = await _getProductHandler.HandleAsync(productId);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(expectedProduct, response.Product);
        _productsManager.Verify(pm => pm.GetProductAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldThrowRpcException_WhenProductNotFound()
    {
        // Arrange
        var productId = "test-product-id";
        _productsManager.Setup(pm => pm.GetProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null); // Simulating that the product is not found

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _getProductHandler.HandleAsync(productId));

        Assert.AreEqual(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldThrowRpcException_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var productId = "test-product-id";
        _productsManager.Setup(pm => pm.GetProductAsync(productId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An unexpected error occurred"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _getProductHandler.HandleAsync(productId));

        Assert.AreEqual(StatusCode.Internal, exception.Status.StatusCode);
        Assert.AreEqual("An unexpected error occurred", exception.Status.Detail);
    }
}
