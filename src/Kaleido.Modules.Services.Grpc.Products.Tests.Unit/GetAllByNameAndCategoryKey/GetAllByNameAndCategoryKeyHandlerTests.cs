using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.GetAllByNameAndCategoryKey;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllByNameAndCategoryKey;

public class GetAllByNameAndCategoryKeyHandlerTests
{
    private readonly AutoMocker _mocker;
    private readonly GetAllByNameAndCategoryKeyHandler _handler;
    private readonly GetAllProductsByNameAndCategoryKeyRequest _validRequest;
    private readonly List<Product> _expectedProducts;

    public GetAllByNameAndCategoryKeyHandlerTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<GetAllByNameAndCategoryKeyHandler>>(NullLogger<GetAllByNameAndCategoryKeyHandler>.Instance);

        _validRequest = new GetAllProductsByNameAndCategoryKeyRequest
        {
            Name = "Test Product",
            CategoryKey = Guid.NewGuid().ToString()
        };
        _expectedProducts = new List<Product>
        {
            new Product { Key = "1", Name = "Test Product 1", CategoryKey = _validRequest.CategoryKey },
            new Product { Key = "2", Name = "Test Product 2", CategoryKey = _validRequest.CategoryKey }
        };

        _mocker.GetMock<IRequestValidator<GetAllProductsByNameAndCategoryKeyRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllProductsByNameAndCategoryKeyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mocker.GetMock<IGetAllByNameAndCategoryKeyManager>()
            .Setup(m => m.GetAllByNameAndCategoryKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_expectedProducts);

        _handler = _mocker.CreateInstance<GetAllByNameAndCategoryKeyHandler>();
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
        _mocker.GetMock<IGetAllByNameAndCategoryKeyManager>()
            .Setup(m => m.GetAllByNameAndCategoryKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        _mocker.GetMock<IRequestValidator<GetAllProductsByNameAndCategoryKeyRequest>>()
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllProductsByNameAndCategoryKeyRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.HandleAsync(_validRequest));
    }

    [Fact]
    public async Task HandleAsync_GenericException_ThrowsRpcExceptionWithInternalError()
    {
        // Arrange
        _mocker.GetMock<IGetAllByNameAndCategoryKeyManager>()
            .Setup(m => m.GetAllByNameAndCategoryKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(() => _handler.HandleAsync(_validRequest));
        Assert.Equal(StatusCode.Internal, exception.StatusCode);
    }
}

