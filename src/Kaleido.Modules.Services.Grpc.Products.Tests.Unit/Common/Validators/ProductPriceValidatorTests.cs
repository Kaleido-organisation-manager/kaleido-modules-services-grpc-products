using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Validators;
public class ProductPriceValidatorTests
{
    private readonly ProductPriceValidator _validator;

    public ProductPriceValidatorTests()
    {
        _validator = new ProductPriceValidator();
    }

    [Fact]
    public async Task ValidateAsync_WithValidPrices_ShouldReturnValidResult()
    {
        // Arrange
        var productPrices = new List<ProductPrice>
        {
            new ProductPrice { Value = 10.99f, CurrencyKey = Guid.NewGuid().ToString() },
            new ProductPrice { Value = 5.50f, CurrencyKey = Guid.NewGuid().ToString() },
            new ProductPrice { Value = 0.01f, CurrencyKey = Guid.NewGuid().ToString() }
        };

        // Act
        var result = await _validator.ValidateAsync(productPrices);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithZeroPrice_ShouldReturnValidResult()
    {
        // Arrange
        var productPrices = new List<ProductPrice>
        {
            new ProductPrice { Value = 0, CurrencyKey = Guid.NewGuid().ToString() }
        };

        // Act
        var result = await _validator.ValidateAsync(productPrices);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithNegativePrice_ShouldReturnInvalidResult()
    {
        // Arrange
        var productPrices = new List<ProductPrice>
        {
            new ProductPrice { Value = -5.99f, CurrencyKey = Guid.NewGuid().ToString() }
        };

        // Act
        var result = await _validator.ValidateAsync(productPrices);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithMixedValidAndInvalidPrices_ShouldReturnInvalidResult()
    {
        // Arrange
        var productPrices = new List<ProductPrice>
        {
            new ProductPrice { Value = 10.99f, CurrencyKey = Guid.NewGuid().ToString() },
            new ProductPrice { Value = -1f, CurrencyKey = Guid.NewGuid().ToString() },
            new ProductPrice { Value = 5.50f, CurrencyKey = Guid.NewGuid().ToString() }
        };

        // Act
        var result = await _validator.ValidateAsync(productPrices);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyList_ShouldReturnValidResult()
    {
        // Arrange
        var productPrices = new List<ProductPrice>();

        // Act
        var result = await _validator.ValidateAsync(productPrices);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidCurrencyKey_ShouldReturnInvalidResult()
    {
        // Arrange
        var productPrices = new List<ProductPrice>
        {
            new ProductPrice { Value = 10.99f, CurrencyKey = "invalid-guid" }
        };

        // Act
        var result = await _validator.ValidateAsync(productPrices);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyCurrencyKey_ShouldReturnInvalidResult()
    {
        // Arrange
        var productPrices = new List<ProductPrice>
        {
            new ProductPrice { Value = 10.99f, CurrencyKey = "" }
        };

        // Act
        var result = await _validator.ValidateAsync(productPrices);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
    }

    [Fact]
    public async Task ValidateAsync_WithWhitespaceCurrencyKey_ShouldReturnInvalidResult()
    {
        // Arrange
        var productPrices = new List<ProductPrice>
        {
            new ProductPrice { Value = 10.99f, CurrencyKey = "   " }
        };

        // Act
        var result = await _validator.ValidateAsync(productPrices);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateCurrencyKeyAsync_WithValidGuid_ShouldReturnValidResult()
    {
        // Arrange
        var currencyKey = Guid.NewGuid().ToString();

        // Act
        var result = await _validator.ValidateCurrencyKeyAsync(currencyKey);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateCurrencyKeyAsync_WithInvalidGuid_ShouldReturnInvalidResult()
    {
        // Arrange
        var currencyKey = "invalid-guid";

        // Act
        var result = await _validator.ValidateCurrencyKeyAsync(currencyKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateCurrencyKeyAsync_WithEmptyString_ShouldReturnInvalidResult()
    {
        // Arrange
        var currencyKey = string.Empty;

        // Act
        var result = await _validator.ValidateCurrencyKeyAsync(currencyKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
    }

    [Fact]
    public async Task ValidateCurrencyKeyAsync_WithWhitespace_ShouldReturnInvalidResult()
    {
        // Arrange
        var currencyKey = "   ";

        // Act
        var result = await _validator.ValidateCurrencyKeyAsync(currencyKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateCurrencyKeyAsync_WithNull_ShouldReturnInvalidResult()
    {
        // Arrange
        string currencyKey = null!;

        // Act
        var result = await _validator.ValidateCurrencyKeyAsync(currencyKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
    }
}

