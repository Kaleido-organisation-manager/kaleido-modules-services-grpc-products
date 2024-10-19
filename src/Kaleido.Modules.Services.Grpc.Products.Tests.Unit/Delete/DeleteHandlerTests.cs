using Grpc.Core;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
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

        _validRequest = new DeleteProductRequest { Key = Guid.NewGuid().ToString() };

        var deletedEntity = new ProductEntity()
        {
            Key = Guid.Parse(_validRequest.Key),
            Name = "Test Product",
            CategoryKey = Guid.NewGuid()
        };

        _mocker.GetMock<IDeleteManager>()
            .Setup(m => m.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedEntity);

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
        Assert.Equal(_validRequest.Key, response.Key);
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

    [Fact]
    public async Task HandleAsync_NotFound_ThrowsRpcExceptionWithNotFoundError()
    {
        _mocker.GetMock<IDeleteManager>()
            .Setup(m => m.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }
}

