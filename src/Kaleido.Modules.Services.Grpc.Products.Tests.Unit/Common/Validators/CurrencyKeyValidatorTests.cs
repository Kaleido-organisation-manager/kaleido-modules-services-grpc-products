using FluentValidation.TestHelper;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Validators;

public class CurrencyKeyValidatorTests
{
    private readonly CurrencyKeyValidator _sut;

    public CurrencyKeyValidatorTests()
    {
        var keyValidator = new KeyValidator();
        _sut = new CurrencyKeyValidator(keyValidator);
    }

    [Fact]
    public async Task Validate_WithValidCurrencyKey_ShouldNotHaveValidationError()
    {
        // Arrange
        var key = Guid.NewGuid().ToString();

        // Act
        var result = await _sut.TestValidateAsync(key);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyString_ShouldHaveValidationError()
    {
        // Arrange
        var key = string.Empty;

        // Act
        var result = await _sut.TestValidateAsync(key);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public async Task Validate_WithInvalidGuid_ShouldHaveValidationError()
    {
        // Arrange
        var key = "not-a-guid";

        // Act
        var result = await _sut.TestValidateAsync(key);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }
}