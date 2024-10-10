using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;
using Moq;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllByCategoryKey;

public class GetAllByCategoryKeyRequestValidatorTests
{
    private readonly GetAllByCategoryKeyRequestValidator _sut;
    private readonly Mock<IProductValidator> _productValidatorMock;
    private readonly GetAllProductsByCategoryKeyRequest _validRequest;

    public GetAllByCategoryKeyRequestValidatorTests()
    {
        _productValidatorMock = new Mock<IProductValidator>();
        _sut = new GetAllByCategoryKeyRequestValidator(_productValidatorMock.Object);

        _validRequest = new GetAllProductsByCategoryKeyRequest { CategoryKey = "valid-key" };

        _productValidatorMock
            .Setup(x => x.ValidateCategoryKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
    public async Task ValidateAsync_WithInvalidCategoryKey_ReturnsInvalidResult()
    {
        // Arrange
        var invalidRequest = new GetAllProductsByCategoryKeyRequest { CategoryKey = "invalid-key" };
        var validationResult = new ValidationResult();
        validationResult.AddRequiredError(["CategoryKey"], "CategoryKey is required");
        _productValidatorMock
            .Setup(x => x.ValidateCategoryKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _sut.ValidateAsync(invalidRequest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_CallsProductValidatorWithCorrectCategoryKey()
    {
        // Act
        await _sut.ValidateAsync(_validRequest);

        // Assert
        _productValidatorMock.Verify(x => x.ValidateCategoryKeyAsync(_validRequest.CategoryKey, It.IsAny<CancellationToken>()), Times.Once);
    }
}
