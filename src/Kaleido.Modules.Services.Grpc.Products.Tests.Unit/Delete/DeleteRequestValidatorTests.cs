using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Delete;
using Moq;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Delete;

public class DeleteRequestValidatorTests
{
    private readonly DeleteRequestValidator _sut;
    private readonly Mock<IProductValidator> _productValidatorMock;
    private readonly DeleteProductRequest _validRequest;

    public DeleteRequestValidatorTests()
    {
        _productValidatorMock = new Mock<IProductValidator>();
        _sut = new DeleteRequestValidator(_productValidatorMock.Object);

        _validRequest = new DeleteProductRequest { Key = "valid-key" };

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
        var invalidRequest = new DeleteProductRequest { Key = "invalid-key" };
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
