using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetProductRevision;
using Moq;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductRevision;

public class GetProductRevisionRequestValidatorTests
{
    private readonly GetProductRevisionRequestValidator _sut;
    private readonly Mock<IProductValidator> _productValidatorMock;
    private readonly GetProductRevisionRequest _validRequest;

    public GetProductRevisionRequestValidatorTests()
    {
        _productValidatorMock = new Mock<IProductValidator>();
        _sut = new GetProductRevisionRequestValidator(_productValidatorMock.Object);

        _validRequest = new GetProductRevisionRequest { Key = "valid-key", Revision = 1 };

        _productValidatorMock
            .Setup(x => x.ValidateKeyFormat(It.IsAny<string>()))
            .Returns(new ValidationResult());
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
        var invalidRequest = new GetProductRevisionRequest { Key = "invalid-key", Revision = 1 };
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["Key"], "Key is required");
        _productValidatorMock
            .Setup(x => x.ValidateKeyFormat(It.IsAny<string>()))
            .Returns(validationResult);

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
        _productValidatorMock.Verify(x => x.ValidateKeyFormat(_validRequest.Key), Times.Once);
    }
}
