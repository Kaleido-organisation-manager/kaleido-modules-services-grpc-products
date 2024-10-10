using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevision;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductPriceRevision;

public class GetProductPriceRevisionHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetProductPriceRevisionHandler _handler;
    private readonly GetProductPriceRevisionRequest _validRequest;
    private readonly ProductPriceRevision _expectedRevision;

    public GetProductPriceRevisionHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetProductPriceRevisionHandler>>(NullLogger<GetProductPriceRevisionHandler>.Instance);

        _validRequest = new GetProductPriceRevisionRequest
        {
            Key = Guid.NewGuid().ToString(),
            CurrencyKey = Guid.NewGuid().ToString(),
            Revision = 1
        };

        _expectedRevision = new ProductPriceRevision
        {
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Value = 9.99f,
            CurrencyKey = _validRequest.CurrencyKey,
            Revision = 1,
            Status = "Active"
        };

        _mocker.GetMock<IGetProductPriceRevisionManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedRevision);

        _mocker.GetMock<IRequestValidator<GetProductPriceRevisionRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductPriceRevisionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = _mocker.CreateInstance<GetProductPriceRevisionHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsProductPriceRevision()
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
        _mocker.GetMock<IRequestValidator<GetProductPriceRevisionRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductPriceRevisionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(_validRequest));
    }

    [Fact]
    public async Task HandleAsync_ManagerReturnsNull_ThrowsRpcExceptionWithNotFound()
    {
        // Arrange
        _mocker.GetMock<IGetProductPriceRevisionManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductPriceRevision?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task HandleAsync_ManagerThrowsException_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IGetProductPriceRevisionManager>()
            .Setup(m => m.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}
