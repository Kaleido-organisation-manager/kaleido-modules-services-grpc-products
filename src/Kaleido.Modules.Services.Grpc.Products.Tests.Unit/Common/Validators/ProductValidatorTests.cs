using FluentValidation.TestHelper;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Microsoft.Extensions.Logging;
using Moq;
using Kaleido.Grpc.Categories;
using Grpc.Core;
using static Kaleido.Grpc.Categories.GrpcCategories;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Validators;

public class ProductValidatorTests
{
    private readonly ProductValidator _sut;
    private readonly Mock<GrpcCategoriesClient> _categoriesClientMock;

    public ProductValidatorTests()
    {
        // Create actual instances of basic validators
        var keyValidator = new KeyValidator();
        var nameValidator = new NameValidator();
        var currencyKeyValidator = new CurrencyKeyValidator(keyValidator);
        var productPriceValidator = new ProductPriceValidator(currencyKeyValidator);

        // Mock only the external dependency (categories client)
        _categoriesClientMock = new Mock<GrpcCategoriesClient>();
        // // Setup happy path for category validation
        _categoriesClientMock.Setup(c => c.GetCategoryAsync(It.IsAny<CategoryRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(new AsyncUnaryCall<CategoryResponse>(Task.FromResult(new CategoryResponse()), Task.FromResult(new Metadata()), null!, null!, null!));

        var categoryKeyValidator = new CategoryKeyValidator(keyValidator, _categoriesClientMock.Object, NullLogger<CategoryKeyValidator>.Instance);

        _sut = new ProductValidator(categoryKeyValidator, nameValidator, productPriceValidator);
    }

    [Fact]
    public async Task Validate_WithValidProduct_ShouldNotHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Test Description",
            Prices = { new ProductPrice { Units = 10, Nanos = 0, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        // Act
        var result = await _sut.TestValidateAsync(product);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Test Description",
            Prices = { new ProductPrice { Units = 10, Nanos = 0, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        // Act
        var result = await _sut.TestValidateAsync(product);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_WithEmptyCategoryKey_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = "",
            Description = "Test Description",
            Prices = { new ProductPrice { Units = 10, Nanos = 0, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        // Act
        var result = await _sut.TestValidateAsync(product);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryKey);
    }

    [Fact]
    public async Task Validate_WithInvalidGuidCategoryKey_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = "not-a-guid",
            Description = "Test Description",
            Prices = { new ProductPrice { Units = 10, Nanos = 0, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        // Act
        var result = await _sut.TestValidateAsync(product);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryKey);
    }

    [Fact]
    public async Task Validate_WithNonExistentCategory_ShouldHaveValidationError()
    {
        // Arrange
        _categoriesClientMock.Setup(c => c.GetCategoryAsync(It.IsAny<CategoryRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Throws(new RpcException(new Status(StatusCode.NotFound, "Category not found")));

        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Test Description",
            Prices = { new ProductPrice { Units = 10, Nanos = 0, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        // Act
        var result = await _sut.TestValidateAsync(product);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryKey);
    }

    [Fact]
    public async Task Validate_WithEmptyPrices_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Test Description"
        };

        // Act
        var result = await _sut.TestValidateAsync(product);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Prices);
    }

    [Fact]
    public async Task Validate_WithDescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = new string('a', 1001),
            Prices = { new ProductPrice { Units = 10, Nanos = 0, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        // Act
        var result = await _sut.TestValidateAsync(product);
    }
}
