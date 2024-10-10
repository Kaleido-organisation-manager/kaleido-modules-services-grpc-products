using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;
using Moq;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductRevisions;

public class GetProductRevisionsRequestValidatorTests
{
    private readonly GetProductRevisionsRequestValidator _sut;
    private readonly Mock<IProductValidator> _productValidatorMock;
    private readonly GetProductRevisionsRequest _validRequest;

    public GetProductRevisionsRequestValidatorTests()
    {
        _productValidatorMock = new Mock<IProductValidator>();
        _sut = new GetProductRevisionsRequestValidator(_productValidatorMock.Object);

        _validRequest = new GetProductRevisionsRequest { Key = "valid-key" };

        _productValidatorMock
            .Setup(x => x.ValidateKeyForRevisionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task ValidateAsync_WithValidRequest_ReturnsValidResult()
    {
        // Act
        var result = await _sut.ValidateAsync(_validRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidKey_ReturnsInvalidResult()
    {
        // Arrange
        var invalidRequest = new GetProductRevisionsRequest { Key = "invalid-key" };
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["Key"], "Key is required");
        _productValidatorMock
            .Setup(x => x.ValidateKeyForRevisionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _sut.ValidateAsync(invalidRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_CallsProductValidatorWithCorrectKey()
    {
        // Act
        await _sut.ValidateAsync(_validRequest);

        // Assert
        _productValidatorMock.Verify(x => x.ValidateKeyForRevisionAsync(_validRequest.Key, It.IsAny<CancellationToken>()), Times.Once);
    }
}
