using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductRevisions;

public class GetProductRevisionsHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetProductRevisionsHandler _handler;
    private readonly GetProductRevisionsRequest _validRequest;
    private readonly List<ProductRevision> _expectedRevisions;

    public GetProductRevisionsHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetProductRevisionsHandler>>(NullLogger<GetProductRevisionsHandler>.Instance);

        _validRequest = new GetProductRevisionsRequest { Key = "123" };
        _expectedRevisions = new List<ProductRevision>
        {
            new ProductRevision { Key = "123", Name = "Product 1", Revision = 1 },
            new ProductRevision { Key = "123", Name = "Product 2", Revision = 2 }
        };

        _mocker.GetMock<IGetProductRevisionsManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedRevisions);

        _mocker.GetMock<IRequestValidator<GetProductRevisionsRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetProductRevisionsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _handler = _mocker.CreateInstance<GetProductRevisionsHandler>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReturnsProductRevisions()
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
        var emptyRevisionList = new List<ProductRevision>();
        _mocker.GetMock<IGetProductRevisionsManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyRevisionList);

        // Act
        var response = await _handler.HandleAsync(_validRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Revisions);
    }

    [Fact]
    public async Task HandleAsync_ExceptionThrown_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IGetProductRevisionsManager>()
            .Setup(m => m.GetAllAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}
