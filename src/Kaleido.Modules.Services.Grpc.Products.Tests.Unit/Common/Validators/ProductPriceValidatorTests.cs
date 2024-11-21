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
            Units = 10,
            Nanos = 0,
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
            Units = -10,
            Nanos = 0,
            CurrencyKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Units);
    }

    [Fact]
    public async Task Validate_WithEmptyCurrencyKey_ShouldHaveValidationError()
    {
        // Arrange
        var price = new ProductPrice
        {
            Units = 10,
            Nanos = 0,
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
            Units = 10,
            Nanos = 0,
            CurrencyKey = "not-a-guid"
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrencyKey);
    }

    [Theory]
    [InlineData(-10, 0)]
    [InlineData(0, -1)]
    [InlineData(0, 100)]
    public async Task Validate_WithInvalidPrice_ShouldHaveValidationError(int units, int nanos)
    {
        // Arrange
        var price = new ProductPrice
        {
            Units = units,
            Nanos = nanos,
            CurrencyKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        if (units < 0)
        {
            result.ShouldHaveValidationErrorFor(x => x.Units);
        }
        else if (nanos < 0 || nanos >= 100)
        {
            result.ShouldHaveValidationErrorFor(x => x.Nanos);
        }
    }


    [Fact]
    public async Task Validate_WithZeroPrice_ShouldBeValid()
    {
        // Arrange
        var price = new ProductPrice
        {
            Units = 0,
            Nanos = 0,
            CurrencyKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.TestValidateAsync(price);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}