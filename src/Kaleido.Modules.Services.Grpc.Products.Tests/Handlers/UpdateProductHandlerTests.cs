using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Exceptions; // Ensure this is included for ValidationException
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Handlers;

[TestClass]
public class UpdateProductHandlerTests
{
    private IUpdateProductHandler _updateProductHandler = null!;
    private Mock<IProductsManager> _productsManager = null!;
    private readonly AutoMocker _mocker = new AutoMocker();

    [TestInitialize]
    public void Initialize()
    {
        _productsManager = _mocker.GetMock<IProductsManager>();
        _updateProductHandler = _mocker.CreateInstance<UpdateProductHandler>();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldReturnUpdatedProduct_WhenSuccessful()
    {
        // Arrange
        var product = new Product { /* initialize properties */ };
        var updatedProduct = new Product { /* initialize properties */ };

        _productsManager.Setup(pm => pm.UpdateProductAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);

        // Act
        var response = await _updateProductHandler.HandleAsync(product);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(updatedProduct, response.Product);
        _productsManager.Verify(pm => pm.UpdateProductAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldThrowRpcException_WhenValidationExceptionOccurs()
    {
        // Arrange
        var product = new Product { /* initialize properties */ };
        _productsManager.Setup(pm => pm.UpdateProductAsync(product, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid product details"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _updateProductHandler.HandleAsync(product));

        Assert.AreEqual(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldThrowRpcException_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var product = new Product { /* initialize properties */ };
        _productsManager.Setup(pm => pm.UpdateProductAsync(product, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An unexpected error occurred"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _updateProductHandler.HandleAsync(product));

        Assert.AreEqual(StatusCode.Internal, exception.Status.StatusCode);
    }
}
