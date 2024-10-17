using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.GetAllByNameAndCategoryKey;
using Moq;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllByNameAndCategoryKey;

public class GetAllByNameAndCategoryKeyRequestValidatorTests
{
    private readonly GetAllByNameAndCategoryKeyRequestValidator _validator;
    private readonly Mock<IProductValidator> _productValidatorMock;

    public GetAllByNameAndCategoryKeyRequestValidatorTests()
    {
        _productValidatorMock = new Mock<IProductValidator>();
        _validator = new GetAllByNameAndCategoryKeyRequestValidator(_productValidatorMock.Object);

        _productValidatorMock
            .Setup(x => x.ValidateCategoryKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task ValidateAsync_NameAndCategoryKeyProvided_ReturnsValidResult()
    {
        // Arrange
        var request = new GetAllProductsByNameAndCategoryKeyRequest
        {
            Name = "Test",
            CategoryKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_NameNotProvided_ReturnsInvalidResult()
    {
        // Arrange
        var request = new GetAllProductsByNameAndCategoryKeyRequest
        {
            CategoryKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_CallsProductValidatorWithCorrectCategoryKey()
    {
        // Arrange
        var request = new GetAllProductsByNameAndCategoryKeyRequest
        {
            Name = "Test",
            CategoryKey = Guid.NewGuid().ToString()
        };

        // Act
        await _validator.ValidateAsync(request);

        // Assert
        _productValidatorMock.Verify(x => x.ValidateCategoryKeyAsync(request.CategoryKey, It.IsAny<CancellationToken>()), Times.Once);
    }
}
