using Grpc.Core;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.GetProductRevision;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductRevision;

public class GetProductRevisionHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetProductRevisionHandler _handler;
    private readonly GetProductRevisionRequest _validRequest;
    private readonly ProductRevision _expectedRevision;

    public GetProductRevisionHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetProductRevisionHandler>>(NullLogger<GetProductRevisionHandler>.Instance);

        _validRequest = new GetProductRevisionRequest
        {
            Key = Guid.NewGuid().ToString(),
            Revision = 1
        };

        _expectedRevision = new ProductRevision
        {
            Key = _validRequest.Key,
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid().ToString(),
            ImageUrl = "http://example.com/image.jpg",
            Revision = 1,
            Status = "Active",
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        _mocker.GetMock<IGetProductRevisionManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedRevision);

        _mocker.GetMock<IRequestValidator<GetProductRevisionRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductRevisionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = _mocker.CreateInstance<GetProductRevisionHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsProductRevision()
    {
        // Act
        var response = await _handler.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(_expectedRevision, response.Revision);
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["Key"], "Key is required");
        _mocker.GetMock<IRequestValidator<GetProductRevisionRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductRevisionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(_validRequest));
    }

    [Fact]
    public async Task HandleAsync_ManagerReturnsNull_ThrowsRpcExceptionWithNotFound()
    {
        // Arrange
        _mocker.GetMock<IGetProductRevisionManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductRevision?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_GenericException_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IGetProductRevisionManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}
