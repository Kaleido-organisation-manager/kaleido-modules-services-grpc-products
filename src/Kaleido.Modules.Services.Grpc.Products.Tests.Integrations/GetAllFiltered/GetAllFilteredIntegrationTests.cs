using Grpc.Core;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetAllFiltered;

[Collection("Infrastructure collection")]
public class GetAllFilteredIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetAllFilteredIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithBothFilters_ShouldReturnMatchingProducts()
    {
        // Arrange
        var category1 = new Category { Name = "Category 1" };
        var category2 = new Category { Name = "Category 2" };
        var categoryResponse1 = await _fixture.CategoriesClient.CreateCategoryAsync(category1);
        var categoryResponse2 = await _fixture.CategoriesClient.CreateCategoryAsync(category2);

        var product1 = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse1.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product2 = new Product
        {
            Name = "Test Another",
            CategoryKey = categoryResponse1.Key,
            Prices = { new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product3 = new Product
        {
            Name = "Test Product Different Category",
            CategoryKey = categoryResponse2.Key,
            Prices = { new ProductPrice { Units = 29, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        await _fixture.Client.CreateProductAsync(product1);
        await _fixture.Client.CreateProductAsync(product2);
        await _fixture.Client.CreateProductAsync(product3);

        // Act
        var response = await _fixture.Client.GetAllProductsFilteredAsync(
            new ProductFilterRequest
            {
                Name = "Test Product",
                CategoryKey = categoryResponse1.Key
            });

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Products);
        Assert.Contains(response.Products, p => p.Product.Name == "Test Product" && p.Product.CategoryKey == categoryResponse1.Key);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithOnlyName_ShouldReturnNameMatches()
    {
        // Arrange
        var category1 = new Category { Name = "Category 1" };
        var category2 = new Category { Name = "Category 2" };
        var categoryResponse1 = await _fixture.CategoriesClient.CreateCategoryAsync(category1);
        var categoryResponse2 = await _fixture.CategoriesClient.CreateCategoryAsync(category2);

        var product1 = new Product
        {
            Name = "Special Product",
            CategoryKey = categoryResponse1.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product2 = new Product
        {
            Name = "Special Item",
            CategoryKey = categoryResponse2.Key,
            Prices = { new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        await _fixture.Client.CreateProductAsync(product1);
        await _fixture.Client.CreateProductAsync(product2);

        // Act
        var response = await _fixture.Client.GetAllProductsFilteredAsync(
            new ProductFilterRequest { Name = "Special" });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.Contains(response.Products, p => p.Product.Name == "Special Product");
        Assert.Contains(response.Products, p => p.Product.Name == "Special Item");
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithOnlyCategoryKey_ShouldReturnCategoryMatches()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product1 = new Product
        {
            Name = "First Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product2 = new Product
        {
            Name = "Second Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        await _fixture.Client.CreateProductAsync(product1);
        await _fixture.Client.CreateProductAsync(product2);

        // Act
        var response = await _fixture.Client.GetAllProductsFilteredAsync(
            new ProductFilterRequest { CategoryKey = categoryResponse.Key });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.All(response.Products, p => Assert.Equal(categoryResponse.Key, p.Product.CategoryKey));
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithNoFilters_ShouldReturnAllProducts()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product1 = new Product
        {
            Name = "First Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product2 = new Product
        {
            Name = "Second Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        await _fixture.Client.CreateProductAsync(product1);
        await _fixture.Client.CreateProductAsync(product2);

        // Act
        var response = await _fixture.Client.GetAllProductsFilteredAsync(new ProductFilterRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithCaseInsensitiveNameMatch_ShouldReturnProducts()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        await _fixture.Client.CreateProductAsync(product);

        // Act
        var response = await _fixture.Client.GetAllProductsFilteredAsync(
            new ProductFilterRequest { Name = "test" });

        var getAllResponse = await _fixture.Client.GetAllProductsAsync(new Kaleido.Grpc.Products.EmptyRequest());

        // Assert
        Assert.Single(getAllResponse.Products);
        Assert.NotNull(response);
        Assert.True(response.Products.All(p => p.Revision.Action != "Deleted"));
        Assert.Single(response.Products);
        Assert.Contains(response.Products, p => p.Product.Name == "Test Product");
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithDeletedProducts_ShouldNotReturnDeletedProducts()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product1 = new Product
        {
            Name = "Test Product One",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product2 = new Product
        {
            Name = "Test Product Two",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var createResponse1 = await _fixture.Client.CreateProductAsync(product1);
        await _fixture.Client.CreateProductAsync(product2);
        await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createResponse1.Key });

        // Act
        var response = await _fixture.Client.GetAllProductsFilteredAsync(
            new ProductFilterRequest
            {
                Name = "Test",
                CategoryKey = categoryResponse.Key
            });

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Products);
        Assert.Contains(response.Products, p => p.Product.Name == "Test Product Two");
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithMultiplePricesPerProduct_ShouldReturnAllPrices()
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        await _fixture.Client.CreateProductAsync(product);

        // Act
        var response = await _fixture.Client.GetAllProductsFilteredAsync(
            new ProductFilterRequest { Name = "Test" });

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Products);
        var returnedProduct = response.Products[0];
        Assert.Equal(2, returnedProduct.Product.Prices.Count);
        Assert.Contains(returnedProduct.Product.Prices, p => p.Price.Units == 9 && p.Price.Nanos == 99);
        Assert.Contains(returnedProduct.Product.Prices, p => p.Price.Units == 19 && p.Price.Nanos == 99);
    }

    [Theory]
    [InlineData("invalid-guid")]
    public async Task GetAllFilteredAsync_WithInvalidCategoryKey_ShouldThrowInvalidArgument(string categoryKey)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetAllProductsFilteredAsync(
                new ProductFilterRequest { CategoryKey = categoryKey }));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetAllFilteredAsync_WithTooLongName_ShouldThrowInvalidArgument()
    {
        // Arrange
        var longName = new string('a', 101); // Exceeds 100 character limit

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetAllProductsFilteredAsync(
                new ProductFilterRequest { Name = longName }));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
}