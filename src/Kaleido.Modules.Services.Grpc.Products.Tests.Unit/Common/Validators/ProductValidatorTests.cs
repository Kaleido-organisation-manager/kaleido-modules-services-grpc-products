using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Validators;

public class ProductValidatorTests
{
    private readonly AutoMocker _mocker;
    private readonly ProductValidator _sut;

    public ProductValidatorTests()
    {
        _mocker = new AutoMocker();
        _sut = _mocker.CreateInstance<ProductValidator>();
    }

    [Fact]
    public async Task ValidateCreateAsync_WithValidProduct_ShouldReturnValidResult()
    {
        // Arrange
        var createProduct = new CreateProduct
        {
            Name = "Valid Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Valid description"
        };

        // Act
        var result = await _sut.ValidateCreateAsync(createProduct);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateCreateAsync_WithEmptyName_ShouldReturnInvalidResult()
    {
        // Arrange
        var createProduct = new CreateProduct
        {
            Name = "",
            CategoryKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.ValidateCreateAsync(createProduct);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateCreateAsync_WithNameExceeding100Characters_ShouldReturnInvalidResult()
    {
        // Arrange
        var createProduct = new CreateProduct
        {
            Name = new string('a', 101),
            CategoryKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.ValidateCreateAsync(createProduct);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateCreateAsync_WithEmptyCategoryKey_ShouldReturnInvalidResult()
    {
        // Arrange
        var createProduct = new CreateProduct
        {
            Name = "Valid Product",
            CategoryKey = ""
        };

        // Act
        var result = await _sut.ValidateCreateAsync(createProduct);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ValidateCreateAsync_WithDescriptionExceeding500Characters_ShouldReturnInvalidResult()
    {
        // Arrange
        var createProduct = new CreateProduct
        {
            Name = "Valid Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = new string('a', 501)
        };

        // Act
        var result = await _sut.ValidateCreateAsync(createProduct);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ValidateUpdateAsync_WithValidProduct_ShouldReturnValidResult()
    {
        // Arrange
        var product = new Product
        {
            Key = Guid.NewGuid().ToString(),
            Name = "Valid Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Valid description"
        };

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductEntity() { Name = "Valid Product", CategoryKey = Guid.NewGuid() });

        // Act
        var result = await _sut.ValidateUpdateAsync(product);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateUpdateAsync_WithEmptyKey_ShouldReturnInvalidResult()
    {
        // Arrange
        var product = new Product
        {
            Key = "",
            Name = "Valid Product",
            CategoryKey = Guid.NewGuid().ToString()
        };

        // Act
        var result = await _sut.ValidateUpdateAsync(product);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ValidateKeyAsync_WithValidKey_ShouldReturnValidResult()
    {
        // Arrange
        var productKey = Guid.NewGuid().ToString();

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductEntity() { Name = "Valid Product", CategoryKey = Guid.NewGuid() });

        // Act
        var result = await _sut.ValidateKeyAsync(productKey);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateKeyAsync_WithEmptyKey_ShouldReturnInvalidResult()
    {
        // Arrange
        var productKey = "";

        // Act
        var result = await _sut.ValidateKeyAsync(productKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ValidateKeyAsync_WithInvalidGuid_ShouldReturnInvalidResult()
    {
        // Arrange
        var productKey = "invalid-guid";

        // Act
        var result = await _sut.ValidateKeyAsync(productKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ValidateKeyAsync_WithNonExistentProduct_ShouldReturnInvalidResult()
    {
        // Arrange
        var productKey = Guid.NewGuid().ToString();

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _sut.ValidateKeyAsync(productKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ValidateCategoryKeyAsync_WithValidKey_ShouldReturnValidResult()
    {
        // Arrange
        var categoryKey = Guid.NewGuid().ToString();

        // Act
        var result = await _sut.ValidateCategoryKeyAsync(categoryKey);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateCategoryKeyAsync_WithEmptyKey_ShouldReturnInvalidResult()
    {
        // Arrange
        var categoryKey = "";

        // Act
        var result = await _sut.ValidateCategoryKeyAsync(categoryKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task ValidateCategoryKeyAsync_WithInvalidGuid_ShouldReturnInvalidResult()
    {
        // Arrange
        var categoryKey = "invalid-guid";

        // Act
        var result = await _sut.ValidateCategoryKeyAsync(categoryKey);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public void ValidateKeyForRevisionAsync_WithValidKey_ShouldReturnValidResult()
    {
        // Arrange
        var productKey = Guid.NewGuid();

        _mocker.GetMock<IProductRepository>()
            .Setup(x => x.GetAllRevisionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductEntity> { new ProductEntity() { Key = productKey, Name = "Valid Product", CategoryKey = Guid.NewGuid() } });

        // Act
        var result = _sut.ValidateKeyFormat(productKey.ToString());

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}