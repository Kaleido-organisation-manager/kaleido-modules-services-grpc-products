using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;
using Moq;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetProductPriceRevisions;

public class GetProductPriceRevisionsRequestValidatorTests
{
    private readonly GetProductPriceRevisionsRequestValidator _sut;
    private readonly Mock<IProductValidator> _productValidatorMock;
    private readonly Mock<IProductPriceValidator> _productPriceValidatorMock;
    private readonly GetProductPriceRevisionsRequest _validRequest;

    public GetProductPriceRevisionsRequestValidatorTests()
    {
        _productValidatorMock = new Mock<IProductValidator>();
        _productPriceValidatorMock = new Mock<IProductPriceValidator>();
        _sut = new GetProductPriceRevisionsRequestValidator(_productValidatorMock.Object, _productPriceValidatorMock.Object);

        _validRequest = new GetProductPriceRevisionsRequest
        {
            Key = Guid.NewGuid().ToString(),
            CurrencyKey = Guid.NewGuid().ToString()
        };

        _productValidatorMock
            .Setup(x => x.ValidateKeyFormat(It.IsAny<string>()))
            .Returns(new ValidationResult());

        _productPriceValidatorMock
            .Setup(x => x.ValidateCurrencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        var invalidKeyValidationResult = new ValidationResult();
        invalidKeyValidationResult.AddRequiredError(["Key"], "Key is required");
        _productValidatorMock
            .Setup(x => x.ValidateKeyFormat(It.IsAny<string>()))
            .Returns(invalidKeyValidationResult);

        // Act
        var result = await _sut.ValidateAsync(_validRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidCurrencyKey_ReturnsInvalidResult()
    {
        // Arrange
        var invalidCurrencyKeyValidationResult = new ValidationResult();
        invalidCurrencyKeyValidationResult.AddRequiredError(["CurrencyKey"], "CurrencyKey is required");
        _productPriceValidatorMock
            .Setup(x => x.ValidateCurrencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidCurrencyKeyValidationResult);

        // Act
        var result = await _sut.ValidateAsync(_validRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidKeyAndCurrencyKey_ReturnsInvalidResultWithMultipleErrors()
    {
        // Arrange
        var invalidKeyValidationResult = new ValidationResult();
        invalidKeyValidationResult.AddRequiredError(["Key"], "Key is required");
        _productValidatorMock
            .Setup(x => x.ValidateKeyFormat(It.IsAny<string>()))
            .Returns(invalidKeyValidationResult);

        var invalidCurrencyKeyValidationResult = new ValidationResult();
        invalidCurrencyKeyValidationResult.AddRequiredError(["CurrencyKey"], "CurrencyKey is required");
        _productPriceValidatorMock
            .Setup(x => x.ValidateCurrencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidCurrencyKeyValidationResult);

        // Act
        var result = await _sut.ValidateAsync(_validRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
    }

    [Fact]
    public async Task ValidateAsync_CallsProductValidatorWithCorrectKey()
    {
        // Act
        await _sut.ValidateAsync(_validRequest);

        // Assert
        _productValidatorMock.Verify(x => x.ValidateKeyFormat(_validRequest.Key), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_CallsProductPriceValidatorWithCorrectCurrencyKey()
    {
        // Act
        await _sut.ValidateAsync(_validRequest);

        // Assert
        _productPriceValidatorMock.Verify(x => x.ValidateCurrencyKeyAsync(_validRequest.CurrencyKey, It.IsAny<CancellationToken>()), Times.Once);
    }
}
