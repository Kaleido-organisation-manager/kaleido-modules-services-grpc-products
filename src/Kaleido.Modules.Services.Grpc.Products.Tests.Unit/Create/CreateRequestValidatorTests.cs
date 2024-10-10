using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Create;

public class CreateRequestValidatorTests
{
    private readonly AutoMocker _mocker;
    private readonly CreateRequestValidator _validator;
    private readonly Mock<IProductValidator> _productValidatorMock;
    private readonly Mock<IProductPriceValidator> _productPriceValidatorMock;

    public CreateRequestValidatorTests()
    {
        _mocker = new AutoMocker();

        _productValidatorMock = _mocker.GetMock<IProductValidator>();
        _productPriceValidatorMock = _mocker.GetMock<IProductPriceValidator>();

        // Happy path setup
        _productValidatorMock
            .Setup(v => v.ValidateCreateAsync(It.IsAny<CreateProduct>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _productPriceValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IEnumerable<ProductPrice>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _validator = _mocker.CreateInstance<CreateRequestValidator>();
    }

    [Fact]
    public async Task ValidateAsync_ValidRequest_ReturnsValidResult()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Product = new CreateProduct
            {
                Name = "Test Product",
                CategoryKey = Guid.NewGuid().ToString(),
                Description = "Test Description"
            }
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidProduct_ReturnsInvalidResult()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Product = new CreateProduct
            {
                Name = "", // Invalid name
                CategoryKey = Guid.NewGuid().ToString(),
                Description = "Test Description"
            }
        };

        var productValidationResult = new ValidationResult();
        productValidationResult.AddRequiredError(new[] { nameof(CreateProduct), nameof(CreateProduct.Name) }, "Name is required");

        _productValidatorMock
            .Setup(v => v.ValidateCreateAsync(It.IsAny<CreateProduct>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productValidationResult);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidProductPrices_ReturnsInvalidResult()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Product = new CreateProduct
            {
                Name = "Test Product",
                CategoryKey = Guid.NewGuid().ToString(),
                Description = "Test Description",
                Prices = { new ProductPrice { Value = -10 } } // Invalid price
            }
        };

        var priceValidationResult = new ValidationResult();
        priceValidationResult.AddInvalidFormatError(new[] { nameof(ProductPrice), nameof(ProductPrice.Value) }, "Price must be non-negative");

        _productPriceValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IEnumerable<ProductPrice>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceValidationResult);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidProductAndPrices_ReturnsInvalidResultWithMultipleErrors()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Product = new CreateProduct
            {
                Name = "", // Invalid name
                CategoryKey = Guid.NewGuid().ToString(),
                Description = "Test Description",
                Prices = { new ProductPrice { Value = -10 } } // Invalid price
            }
        };

        var productValidationResult = new ValidationResult();
        productValidationResult.AddRequiredError(new[] { nameof(CreateProduct), nameof(CreateProduct.Name) }, "Name is required");

        _productValidatorMock
            .Setup(v => v.ValidateCreateAsync(It.IsAny<CreateProduct>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productValidationResult);

        var priceValidationResult = new ValidationResult();
        priceValidationResult.AddInvalidFormatError(new[] { nameof(ProductPrice), nameof(ProductPrice.Value) }, "Price must be non-negative");

        _productPriceValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<IEnumerable<ProductPrice>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(priceValidationResult);

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count());
    }
}
