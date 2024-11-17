using FluentValidation.TestHelper;
using Grpc.Core;
using Kaleido.Grpc.Categories;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static Kaleido.Grpc.Categories.GrpcCategories;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Validators;

public class CategoryKeyValidatorTests
{
    private readonly CategoryKeyValidator _sut;
    private readonly Mock<GrpcCategoriesClient> _categoriesClientMock;

    public CategoryKeyValidatorTests()
    {
        var keyValidator = new KeyValidator();
        _categoriesClientMock = new Mock<GrpcCategoriesClient>();

        // Setup happy path
        _categoriesClientMock.Setup(c => c.GetCategoryAsync(It.IsAny<CategoryRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(new AsyncUnaryCall<CategoryResponse>(Task.FromResult(new CategoryResponse()), Task.FromResult(new Metadata()), null!, null!, null!));

        _sut = new CategoryKeyValidator(keyValidator, _categoriesClientMock.Object, NullLogger<CategoryKeyValidator>.Instance);
    }

    [Fact]
    public async Task Validate_WithValidCategoryKey_ShouldNotHaveValidationError()
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

    [Fact]
    public async Task Validate_WithNonExistentCategory_ShouldHaveValidationError()
    {
        // Arrange
        _categoriesClientMock.Setup(c => c.GetCategoryAsync(It.IsAny<CategoryRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Throws(new RpcException(new Status(StatusCode.NotFound, "Category not found")));

        var key = Guid.NewGuid().ToString();

        // Act
        var result = await _sut.TestValidateAsync(key);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public async Task Validate_WithServiceError_ShouldHaveValidationError()
    {
        // Arrange
        _categoriesClientMock.Setup(c => c.GetCategoryAsync(It.IsAny<CategoryRequest>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Throws(new RpcException(new Status(StatusCode.Internal, "Internal server error")));

        var key = Guid.NewGuid().ToString();

        // Act
        var result = await _sut.TestValidateAsync(key);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }
}