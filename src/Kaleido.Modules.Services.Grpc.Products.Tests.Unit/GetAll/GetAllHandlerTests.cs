using Grpc.Core;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.GetAll;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAll;

public class GetAllHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllHandler _handler;
    private readonly List<Product> _expectedProducts;

    public GetAllHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetAllHandler>>(NullLogger<GetAllHandler>.Instance);

        _expectedProducts = new List<Product>
        {
            new Product { Key = "1", Name = "Product 1" },
            new Product { Key = "2", Name = "Product 2" }
        };

        _mocker.GetMock<IGetAllManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProducts);

        _mocker.GetMock<IRequestValidator<GetAllProductsRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllProductsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = _mocker.CreateInstance<GetAllHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsAllProducts()
    {
        // Arrange
        var request = new GetAllProductsRequest();

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(_expectedProducts.Count, response.Products.Count);
        Assert.Equal(_expectedProducts, response.Products);
    }

    [Fact]
    public async Task HandleAsync_EmptyProductList_ReturnsEmptyResponse()
    {
        // Arrange
        var request = new GetAllProductsRequest();
        var emptyProductList = new List<Product>();
        _mocker.GetMock<IGetAllManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyProductList);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Products);
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var request = new GetAllProductsRequest();
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["Products"], "Products are required");
        _mocker.GetMock<IRequestValidator<GetAllProductsRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllProductsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(request));
    }

    [Fact]
    public async Task HandleAsync_ExceptionThrown_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        var request = new GetAllProductsRequest();
        _mocker.GetMock<IGetAllManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(request));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}
