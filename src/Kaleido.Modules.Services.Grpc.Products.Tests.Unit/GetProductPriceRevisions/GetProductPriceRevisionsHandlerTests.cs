using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductPriceRevisions;

public class GetProductPriceRevisionsHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetProductPriceRevisionsHandler _handler;
    private readonly GetProductPriceRevisionsRequest _validRequest;
    private readonly List<ProductPriceRevision> _expectedRevisions;

    public GetProductPriceRevisionsHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetProductPriceRevisionsHandler>>(NullLogger<GetProductPriceRevisionsHandler>.Instance);

        _validRequest = new GetProductPriceRevisionsRequest
        {
            Key = Guid.NewGuid().ToString(),
            CurrencyKey = Guid.NewGuid().ToString()
        };

        _expectedRevisions = new List<ProductPriceRevision>
        {
            new ProductPriceRevision { Revision = 1, Value = 9.99f },
            new ProductPriceRevision { Revision = 2, Value = 10.99f }
        };

        _mocker.GetMock<IGetProductPriceRevisionsManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedRevisions);

        _mocker.GetMock<IRequestValidator<GetProductPriceRevisionsRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductPriceRevisionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = _mocker.CreateInstance<GetProductPriceRevisionsHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsProductPriceRevisions()
    {
        // Act
        var response = await _handler.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(_expectedRevisions.Count, response.Revisions.Count);
        Assert.Equal(_expectedRevisions, response.Revisions);
    }

    [Fact]
    public async Task HandleAsync_EmptyRevisionList_ReturnsEmptyResponse()
    {
        // Arrange
        var emptyRevisionList = new List<ProductPriceRevision>();
        _mocker.GetMock<IGetProductPriceRevisionsManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyRevisionList);

        // Act
        var response = await _handler.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Revisions);
    }

    [Fact]
    public async Task HandleAsync_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["Key"], "Key is required");
        _mocker.GetMock<IRequestValidator<GetProductPriceRevisionsRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductPriceRevisionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(_validRequest));
    }

    [Fact]
    public async Task HandleAsync_GenericException_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IGetProductPriceRevisionsManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}
