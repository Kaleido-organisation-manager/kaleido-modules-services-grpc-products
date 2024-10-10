using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Update;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Update;

public class UpdateRequestValidatorTests
{
    private readonly AutoMocker _mocker;
    private readonly UpdateRequestValidator _validator;
    private readonly Mock<IProductValidator> _productValidatorMock;
    private readonly UpdateProductRequest _validRequest;

    public UpdateRequestValidatorTests()
    {
        _mocker = new AutoMocker();
        _productValidatorMock = _mocker.GetMock<IProductValidator>();
        _validator = _mocker.CreateInstance<UpdateRequestValidator>();

        _validRequest = new UpdateProductRequest
        {
            Key = "valid-key",
            Product = new Product
            {
                Key = "valid-key",
                Name = "Valid Product",
                CategoryKey = "valid-category-key"
            }
        };

        // Happy path setup
        _productValidatorMock
            .Setup(v => v.ValidateKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _productValidatorMock
            .Setup(v => v.ValidateUpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task ValidateAsync_ValidRequest_ReturnsValidResult()
    {
        // Act
        var result = await _validator.ValidateAsync(_validRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidKey_ReturnsInvalidResult()
    {
        // Arrange
        var invalidKeyValidationResult = new ValidationResult();
        invalidKeyValidationResult.AddRequiredError(["Key"], "Key is required");
        _productValidatorMock
            .Setup(v => v.ValidateKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidKeyValidationResult);

        // Act
        var result = await _validator.ValidateAsync(_validRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidProduct_ReturnsInvalidResult()
    {
        // Arrange
        var invalidProductValidationResult = new ValidationResult();
        invalidProductValidationResult.AddRequiredError(["Product", "Name"], "Name is required");
        _productValidatorMock
            .Setup(v => v.ValidateUpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidProductValidationResult);

        // Act
        var result = await _validator.ValidateAsync(_validRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidKeyAndProduct_ReturnsInvalidResultWithMultipleErrors()
    {
        // Arrange
        var invalidKeyValidationResult = new ValidationResult();
        invalidKeyValidationResult.AddRequiredError(["Key"], "Key is required");
        _productValidatorMock
            .Setup(v => v.ValidateKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidKeyValidationResult);

        var invalidProductValidationResult = new ValidationResult();
        invalidProductValidationResult.AddRequiredError(["Product", "Name"], "Name is required");
        _productValidatorMock
            .Setup(v => v.ValidateUpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidProductValidationResult);

        // Act
        var result = await _validator.ValidateAsync(_validRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
    }

    [Fact]
    public async Task ValidateAsync_CallsProductValidatorWithCorrectKey()
    {
        // Act
        await _validator.ValidateAsync(_validRequest);

        // Assert
        _productValidatorMock.Verify(x => x.ValidateKeyAsync(_validRequest.Key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_CallsProductValidatorWithCorrectProduct()
    {
        // Act
        await _validator.ValidateAsync(_validRequest);

        // Assert
        _productValidatorMock.Verify(x => x.ValidateUpdateAsync(_validRequest.Product, It.IsAny<CancellationToken>()), Times.Once);
    }
}
