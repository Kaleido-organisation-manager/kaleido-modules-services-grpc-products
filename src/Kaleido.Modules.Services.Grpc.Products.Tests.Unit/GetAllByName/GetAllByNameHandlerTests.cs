using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.GetAllByName;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllByName;

public class GetAllByNameHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllByNameHandler _handler;
    private readonly GetAllProductsByNameRequest _validRequest;
    private readonly List<Product> _expectedProducts;

    public GetAllByNameHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetAllByNameHandler>>(NullLogger<GetAllByNameHandler>.Instance);

        _validRequest = new GetAllProductsByNameRequest { Name = "Test Product" };
        _expectedProducts = new List<Product>
        {
            new Product { Key = "1", Name = "Test Product 1" },
            new Product { Key = "2", Name = "Test Product 2" }
        };

        _mocker.GetMock<IGetAllByNameManager>()
            .Setup(m => m.GetAllByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProducts);

        _mocker.GetMock<IRequestValidator<GetAllProductsByNameRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllProductsByNameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = _mocker.CreateInstance<GetAllByNameHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsProducts()
    {
        // Act
        var response = await _handler.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(_expectedProducts.Count, response.Products.Count);
        Assert.Equal(_expectedProducts, response.Products);
    }

    [Fact]
    public async Task HandleAsync_EmptyProductList_ReturnsEmptyResponse()
    {
        // Arrange
        _mocker.GetMock<IGetAllByNameManager>()
            .Setup(m => m.GetAllByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var response = await _handler.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Products);
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["Name"], "Name is required");
        _mocker.GetMock<IRequestValidator<GetAllProductsByNameRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllProductsByNameRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(_validRequest));
    }

    [Fact]
    public async Task HandleAsync_ExceptionThrown_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IGetAllByNameManager>()
            .Setup(m => m.GetAllByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}