using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Handlers;

[TestClass]
public class GetAllProductsHandlerTests
{
    private IGetAllProductsHandler _getAllProductsHandler = null!;
    private Mock<IProductsManager> _productsManager = null!;
    private Mock<ILogger<GetAllProductsHandler>> _logger = null!;
    private readonly AutoMocker _mocker = new AutoMocker();

    [TestInitialize]
    public void Initialize()
    {
        _logger = _mocker.GetMock<ILogger<GetAllProductsHandler>>();
        _productsManager = _mocker.GetMock<IProductsManager>();
        _getAllProductsHandler = _mocker.CreateInstance<GetAllProductsHandler>();
    }

    [TestMethod]
    public async Task HandleAsync_ShouldReturnProducts_WhenSuccessful()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { /* initialize properties */ },
            new Product { /* initialize properties */ }
        };

        _productsManager.Setup(pm => pm.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var response = await _getAllProductsHandler.HandleAsync();

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Products.Count > 0);
        Assert.AreEqual(products.Count, response.Products.Count);
        _productsManager.Verify(pm => pm.GetAllProductsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HandleAsync_ShouldHandleExceptions_WhenErrorOccurs()
    {
        // Arrange
        _productsManager.Setup(pm => pm.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("An error occurred while retrieving products"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<RpcException>(
            async () => await _getAllProductsHandler.HandleAsync());

        Assert.AreEqual(StatusCode.Internal, exception.Status.StatusCode);
    }
}
