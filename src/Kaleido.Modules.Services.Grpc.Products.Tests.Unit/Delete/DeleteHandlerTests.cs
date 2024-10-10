using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Delete;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Delete;

public class DeleteProductHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly DeleteHandler _handler;
    private readonly DeleteProductRequest _validRequest;

    public DeleteProductHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<DeleteHandler>>(NullLogger<DeleteHandler>.Instance);

        _validRequest = new DeleteProductRequest { Key = "123" };

        _mocker.GetMock<IDeleteManager>()
            .Setup(m => m.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mocker.GetMock<IRequestValidator<DeleteProductRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = _mocker.CreateInstance<DeleteHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsDeletedProductKey()
    {
        // Act
        var response = await _handler.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("123", response.Key);
    }

    [Fact]
    public async Task HandleAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var invalidRequest = new DeleteProductRequest { Key = "invalid" };
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["Key"], "Key is required");
        _mocker.GetMock<IRequestValidator<DeleteProductRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteProductRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(invalidRequest));
    }

    [Fact]
    public async Task HandleAsync_GenericException_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IDeleteManager>()
            .Setup(m => m.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}

