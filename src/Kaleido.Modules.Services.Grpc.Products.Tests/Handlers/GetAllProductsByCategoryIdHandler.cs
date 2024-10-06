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
public class GetAllProductsByCategoryIdHandlerTests
{
    private IGetAllProductsByCategoryIdHandler _getAllProductsHandler = null!;
    private Mock<IProductsManager> _productsManager = null!;
    private Mock<ILogger<GetAllProductsByCategoryIdHandler>> _logger = null!;
    private readonly AutoMocker _mocker = new AutoMocker();

    [TestInitialize]
    public void Initialize()
    {
        _logger = _mocker.GetMock<ILogger<GetAllProductsByCategoryIdHandler>>();
        _productsManager = _mocker.GetMock<IProductsManager>();
        _getAllProductsHandler = _mocker.CreateInstance<GetAllProductsByCategoryIdHandler>();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldReturnProducts_WhenSuccessful()
    {
        // Arrange
        var categoryId = "test-category-id";
        var products = new List<Product>
        {
            new Product { /* initialize properties */ },
            new Product { /* initialize properties */ }
        };

        _productsManager.Setup(pm => pm.GetAllProductsByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var response = await _getAllProductsHandler.HandleAsync(categoryId);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Products.Count > 0);
        Assert.AreEqual(products.Count, response.Products.Count);
        _productsManager.Verify(pm => pm.GetAllProductsByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldThrowRpcException_WhenValidationExceptionOccurs()
    {
        // Arrange
        var categoryId = "test-category-id";
        _productsManager.Setup(pm => pm.GetAllProductsByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Invalid category ID"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _getAllProductsHandler.HandleAsync(categoryId));

        Assert.AreEqual(StatusCode.InvalidArgument, exception.Status.StatusCode);
        Assert.AreEqual("Invalid category ID", exception.Status.Detail);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldThrowRpcException_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var categoryId = "test-category-id";
        _productsManager.Setup(pm => pm.GetAllProductsByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An unexpected error occurred"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _getAllProductsHandler.HandleAsync(categoryId));

        Assert.AreEqual(StatusCode.Internal, exception.Status.StatusCode);
        Assert.AreEqual("An unexpected error occurred", exception.Status.Detail);
    }
}
