using Grpc.Core;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Get;

[Collection("Infrastructure collection")]
public class GetIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnProduct()
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
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        var createResponse = await _fixture.Client.CreateProductAsync(product);

        // Act
        var getResponse = await _fixture.Client.GetProductAsync(new ProductRequest { Key = createResponse.Key });

        // Assert
        Assert.NotNull(getResponse);
        Assert.Equal(createResponse.Key, getResponse.Key);
        Assert.Equal("Test Product", getResponse.Product.Name);
        Assert.Equal("Test Description", getResponse.Product.Description);
        Assert.Equal(categoryResponse.Key, getResponse.Product.CategoryKey);
        Assert.Single(getResponse.Product.Prices);
        Assert.Equal(9.99f, getResponse.Product.Prices[0].Price.Value);
    }

    [Fact]
    public async Task GetAsync_WithMultiplePrices_ShouldReturnAllPrices()
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
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Value = 19.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        var createResponse = await _fixture.Client.CreateProductAsync(product);

        // Act
        var getResponse = await _fixture.Client.GetProductAsync(new ProductRequest { Key = createResponse.Key });

        // Assert
        Assert.NotNull(getResponse);
        Assert.Equal(2, getResponse.Product.Prices.Count);
        Assert.Contains(getResponse.Product.Prices, p => p.Price.Value == 9.99f);
        Assert.Contains(getResponse.Product.Prices, p => p.Price.Value == 19.99f);
    }

    [Fact]
    public async Task GetAsync_DeletedProduct_ShouldThrowNotFound()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var createResponse = await _fixture.Client.CreateProductAsync(product);
        await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createResponse.Key });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetProductAsync(new ProductRequest { Key = createResponse.Key }));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetAsync_NonExistentProduct_ShouldThrowNotFound()
    {
        // Arrange
        var request = new ProductRequest { Key = Guid.NewGuid().ToString() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetProductAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    public async Task GetAsync_InvalidKey_ShouldThrowInvalidArgument(string key)
    {
        // Arrange
        var request = new ProductRequest { Key = key };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetProductAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }
}
