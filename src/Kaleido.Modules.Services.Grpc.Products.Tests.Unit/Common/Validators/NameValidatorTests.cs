using FluentValidation.TestHelper;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Validators;

public class NameValidatorTests
{
    private readonly NameValidator _sut;

    public NameValidatorTests()
    {
        _sut = new NameValidator();
    }

    [Fact]
    public async Task Validate_WithValidName_ShouldNotHaveValidationError()
    {
        // Arrange
        var name = "Valid Product Name";

        // Act
        var result = await _sut.TestValidateAsync(name);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyString_ShouldHaveValidationError()
    {
        // Arrange
        var name = string.Empty;

        // Act
        var result = await _sut.TestValidateAsync(name);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public async Task Validate_WithTooLongName_ShouldHaveValidationError()
    {
        // Arrange
        var name = new string('a', 101); // 101 characters

        // Act
        var result = await _sut.TestValidateAsync(name);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }
}