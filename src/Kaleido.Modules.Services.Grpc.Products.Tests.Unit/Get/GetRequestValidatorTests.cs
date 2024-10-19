using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Get;
using Moq;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Get;

public class GetRequestValidatorTests
{
    private readonly GetRequestValidator _sut;
    private readonly Mock<IProductValidator> _productValidatorMock;
    private readonly GetProductRequest _validRequest;

    public GetRequestValidatorTests()
    {
        _productValidatorMock = new Mock<IProductValidator>();
        _sut = new GetRequestValidator(_productValidatorMock.Object);

        _validRequest = new GetProductRequest { Key = "valid-key" };

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
        var invalidRequest = new GetProductRequest { Key = "invalid-key" };
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
