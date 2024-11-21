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
                    Units = 9,
                    Nanos = 99,
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
        Assert.Equal(9, response.Product.Prices[0].Price.Units);
        Assert.Equal(99, response.Product.Prices[0].Price.Nanos);
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act
        var response = await _fixture.Client.CreateProductAsync(product);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Product.Prices.Count);
        Assert.Contains(response.Product.Prices, p => p.Price.Units == 9 && p.Price.Nanos == 99);
        Assert.Contains(response.Product.Prices, p => p.Price.Units == 19 && p.Price.Nanos == 99);
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateProductAsync(product));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(-1, -1)]
    public async Task CreateAsync_InvalidPriceValue_ShouldThrow(int units, int nanos)
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
                new ProductPrice { Units = units, Nanos = nanos, CurrencyKey = Guid.NewGuid().ToString() }
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = currencyKey }
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.CreateProductAsync(product));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }
}
