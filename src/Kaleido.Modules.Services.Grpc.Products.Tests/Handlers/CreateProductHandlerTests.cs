using Kaleido.Modules.Services.Grpc.Products.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Grpc.Core;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Handlers;

[TestClass]
public class CreateProductHandlerTests
{
    private ICreateProductHandler _createProductHandler = null!;
    private Mock<IProductsManager> _productsManager = null!;
    private Mock<ILogger<CreateProductHandler>> _logger = null!;
    private readonly AutoMocker _mocker = new AutoMocker();

    [TestInitialize]
    public void Initialize()
    {
        _logger = _mocker.GetMock<ILogger<CreateProductHandler>>();
        _productsManager = _mocker.GetMock<IProductsManager>();
        _createProductHandler = _mocker.CreateInstance<CreateProductHandler>();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldReturnProduct_WhenSuccessful()
    {
        // Arrange
        var createProduct = new CreateProduct { /* initialize properties */ };
        var expectedProduct = new Product { /* initialize properties */ };

        _productsManager.Setup(pm => pm.CreateProductAsync(createProduct, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var response = await _createProductHandler.HandleAsync(createProduct);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(expectedProduct, response.Product);
        _productsManager.Verify(pm => pm.CreateProductAsync(createProduct, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldThrowRpcException_WhenValidationExceptionOccurs()
    {
        // Arrange
        var createProduct = new CreateProduct { /* initialize properties */ };
        _productsManager.Setup(pm => pm.CreateProductAsync(createProduct, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid product details"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _createProductHandler.HandleAsync(createProduct));

        Assert.AreEqual(StatusCode.InvalidArgument, exception.Status.StatusCode);
        Assert.AreEqual("Invalid product details", exception.Status.Detail);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldThrowRpcException_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var createProduct = new CreateProduct { /* initialize properties */ };
        _productsManager.Setup(pm => pm.CreateProductAsync(createProduct, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An unexpected error occurred"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _createProductHandler.HandleAsync(createProduct));

        Assert.AreEqual(StatusCode.Internal, exception.Status.StatusCode);
        Assert.AreEqual("An unexpected error occurred", exception.Status.Detail);
    }
}
