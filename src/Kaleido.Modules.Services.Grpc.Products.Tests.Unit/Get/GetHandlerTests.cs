using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Get;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Get;

public class GetHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetHandler _handler;
    private readonly GetProductRequest _validRequest;
    private readonly Product _expectedProduct;

    public GetHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetHandler>>(NullLogger<GetHandler>.Instance);

        _validRequest = new GetProductRequest { Key = "123" };
        _expectedProduct = new Product { Key = "123", Name = "Test Product" };

        _mocker.GetMock<IRequestValidator<GetProductRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mocker.GetMock<IGetManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProduct);

        _handler = _mocker.CreateInstance<GetHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsProduct()
    {
        // Act
        var response = await _handler.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(_expectedProduct, response.Product);
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["Key"], "Key is required");
        _mocker.GetMock<IRequestValidator<GetProductRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(_validRequest));
    }

    [Fact]
    public async Task HandleAsync_ManagerReturnsNull_ThrowsRpcExceptionWithNotFound()
    {
        // Arrange
        _mocker.GetMock<IGetManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IGetManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}

