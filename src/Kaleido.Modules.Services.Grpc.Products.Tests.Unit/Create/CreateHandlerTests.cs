using Grpc.Core;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Create;

public class CreateProductHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly CreateHandler _handler;
    private readonly CreateProductRequest _validRequest;
    private readonly Product _expectedProduct;

    public CreateProductHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<CreateHandler>>(NullLogger<CreateHandler>.Instance);
        _handler = _mocker.CreateInstance<CreateHandler>();

        _validRequest = new CreateProductRequest
        {
            Product = new CreateProduct { Name = "Test Product" }
        };
        _expectedProduct = new Product { Key = "123", Name = "Test Product" };

        _mocker.GetMock<IRequestValidator<CreateProductRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<CreateProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mocker.GetMock<ICreateManager>()
            .Setup(m => m.CreateAsync(It.IsAny<CreateProduct>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProduct);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsCreatedProduct()
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
        validationResult.AddRequiredError(["Name"], "Name is required");
        _mocker.GetMock<IRequestValidator<CreateProductRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<CreateProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(_validRequest));
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<ICreateManager>()
            .Setup(m => m.CreateAsync(It.IsAny<CreateProduct>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
    }
}