using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Update;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Update;

public class UpdateHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly UpdateHandler _handler;
    private readonly UpdateProductRequest _validRequest;
    private readonly Product _expectedProduct;

    public UpdateHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<UpdateHandler>>(NullLogger<UpdateHandler>.Instance);
        _handler = _mocker.CreateInstance<UpdateHandler>();

        _validRequest = new UpdateProductRequest
        {
            Key = "123",
            Product = new Product { Key = "123", Name = "Updated Product" }
        };
        _expectedProduct = new Product { Key = "123", Name = "Updated Product" };

        _mocker.GetMock<IRequestValidator<UpdateProductRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mocker.GetMock<IUpdateManager>()
            .Setup(m => m.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProduct);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsUpdatedProduct()
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
        _mocker.GetMock<IRequestValidator<UpdateProductRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(_validRequest));
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IUpdateManager>()
            .Setup(m => m.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}
