using Grpc.Core;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Create;

[Collection("Infrastructure collection")]
public class CreateIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public CreateIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProduct()
    {
        // Arrange
        var category = new Category
        {
            Name = "Test Category"
        };

        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice
                {
                    Value = 9.99f,
                    CurrencyKey = Guid.NewGuid().ToString()
                }
            }
        };

        // Act
        var response = await _fixture.Client.CreateProductAsync(product);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Test Product", response.Product.Name);
        Assert.Equal("Test Description", response.Product.Description);
        Assert.Equal(categoryResponse.Key, response.Product.CategoryKey);
        Assert.NotNull(response.Revision);
        Assert.Equal("Created", response.Revision.Action);
        Assert.Equal(1, response.Revision.Revision);
        Assert.Single(response.Product.Prices);
        Assert.Equal(9.99f, response.Product.Prices[0].Price.Value);
    }

    [Fact]
    public async Task CreateAsync_WithMultiplePrices_ShouldCreateProduct()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Value = 19.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act
        var response = await _fixture.Client.CreateProductAsync(product);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Product.Prices.Count);
        Assert.Contains(response.Product.Prices, p => p.Price.Value == 9.99f);
        Assert.Contains(response.Product.Prices, p => p.Price.Value == 19.99f);
    }

    [Fact]
    public async Task CreateAsync_WithoutDescription_ShouldCreateProduct()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act
        var response = await _fixture.Client.CreateProductAsync(product);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Test Product", response.Product.Name);
        Assert.Empty(response.Product.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAsync_InvalidName_ShouldThrow(string name)
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = name,
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateProductAsync(product));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    public async Task CreateAsync_InvalidCategoryKey_ShouldThrow(string categoryKey)
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryKey,
            Prices =
            {
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateProductAsync(product));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-9.99)]
    public async Task CreateAsync_InvalidPriceValue_ShouldThrow(float price)
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = price, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateProductAsync(product));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    public async Task CreateAsync_InvalidCurrencyKey_ShouldThrow(string currencyKey)
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = 9.99f, CurrencyKey = currencyKey }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateProductAsync(product));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_DescriptionTooLong_ShouldThrow()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product",
            Description = new string('a', 1001), // Exceeds 1000 character limit
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateProductAsync(product));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }
}
