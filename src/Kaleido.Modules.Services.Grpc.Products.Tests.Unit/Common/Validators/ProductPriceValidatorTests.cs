using FluentValidation.TestHelper;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Validators;

public class ProductPriceValidatorTests
{
    private readonly ProductPriceValidator _sut;

    public ProductPriceValidatorTests()
    {
        // Create actual instances of validators
        var keyValidator = new KeyValidator();
        var currencyKeyValidator = new CurrencyKeyValidator(keyValidator);
        _sut = new ProductPriceValidator(currencyKeyValidator);
    }

    [Fact]
    public async Task Validate_WithValidPrice_ShouldNotHaveValidationError()
    {
        // Arrange
        var price = new ProductPrice
        {
            Value = 10.0f,
            CurrencyKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithNegativeValue_ShouldHaveValidationError()
    {
        // Arrange
        var price = new ProductPrice
        {
            Value = -10.0f,
            CurrencyKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public async Task Validate_WithZeroValue_ShouldHaveValidationError()
    {
        // Arrange
        var price = new ProductPrice
        {
            Value = 0,
            CurrencyKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Value);
    }

    [Fact]
    public async Task Validate_WithEmptyCurrencyKey_ShouldHaveValidationError()
    {
        // Arrange
        var price = new ProductPrice
        {
            Value = 10.0f,
            CurrencyKey = ""
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrencyKey);
    }

    [Fact]
    public async Task Validate_WithInvalidGuidCurrencyKey_ShouldHaveValidationError()
    {
        // Arrange
        var price = new ProductPrice
        {
            Value = 10.0f,
            CurrencyKey = "not-a-guid"
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrencyKey);
    }
}