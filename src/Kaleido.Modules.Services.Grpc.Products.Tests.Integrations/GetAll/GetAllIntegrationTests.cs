using Grpc.Core;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetAll;

[Collection("Infrastructure collection")]
public class GetAllIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetAllIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var response = await _fixture.Client.GetAllProductsAsync(new Kaleido.Grpc.Products.EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Products);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleProducts_ShouldReturnAllProducts()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product1 = new Product
        {
            Name = "Test Product 1",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product2 = new Product
        {
            Name = "Test Product 2",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Value = 19.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        await _fixture.Client.CreateProductAsync(product1);
        await _fixture.Client.CreateProductAsync(product2);

        // Act
        var response = await _fixture.Client.GetAllProductsAsync(new Kaleido.Grpc.Products.EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.Contains(response.Products, p => p.Product.Name == "Test Product 1");
        Assert.Contains(response.Products, p => p.Product.Name == "Test Product 2");
    }

    [Fact]
    public async Task GetAllAsync_WithDeletedProducts_ShouldNotReturnDeletedProducts()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product1 = new Product
        {
            Name = "Test Product 1",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product2 = new Product
        {
            Name = "Test Product 2",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Value = 19.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var createResponse1 = await _fixture.Client.CreateProductAsync(product1);
        await _fixture.Client.CreateProductAsync(product2);
        await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createResponse1.Key });

        // Act
        var response = await _fixture.Client.GetAllProductsAsync(new Kaleido.Grpc.Products.EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Products);
        Assert.Contains(response.Products, p => p.Product.Name == "Test Product 2");
        Assert.DoesNotContain(response.Products, p => p.Product.Name == "Test Product 1");
    }

    [Fact]
    public async Task GetAllAsync_WithMultiplePricesPerProduct_ShouldReturnAllPrices()
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

        await _fixture.Client.CreateProductAsync(product);

        // Act
        var response = await _fixture.Client.GetAllProductsAsync(new Kaleido.Grpc.Products.EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Products);
        var returnedProduct = response.Products[0];
        Assert.Equal(2, returnedProduct.Product.Prices.Count);
        Assert.Contains(returnedProduct.Product.Prices, p => p.Price.Value == 9.99f);
        Assert.Contains(returnedProduct.Product.Prices, p => p.Price.Value == 19.99f);
    }

    // TODO: Uncomment when the UpdateProduct method is implemented
    // [Fact]
    // public async Task GetAllAsync_WithDeletedPrices_ShouldNotReturnDeletedPrices()
    // {
    //     // Arrange
    //     var category = new Category { Name = "Test Category" };
    //     var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

    //     var product = new Product
    //     {
    //         Name = "Test Product",
    //         CategoryKey = categoryResponse.Key,
    //         Prices =
    //         {
    //             new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() }
    //         }
    //     };

    //     var createResponse = await _fixture.Client.CreateProductAsync(product);

    //     // Update product with new price (which effectively "deletes" the old price)
    //     var updateProduct = new Product
    //     {
    //         Name = "Test Product",
    //         CategoryKey = categoryResponse.Key,
    //         Prices =
    //         {
    //             new ProductPrice { Value = 19.99f, CurrencyKey = Guid.NewGuid().ToString() }
    //         }
    //     };
    //     await _fixture.Client.UpdateProductAsync(new UpdateProductRequest
    //     {
    //         Key = createResponse.Key,
    //         Product = updateProduct
    //     });

    //     // Act
    //     var response = await _fixture.Client.GetAllProductsAsync(new EmptyRequest());

    //     // Assert
    //     Assert.NotNull(response);
    //     Assert.Single(response.Products);
    //     var returnedProduct = response.Products[0];
    //     Assert.Single(returnedProduct.Product.Prices);
    //     Assert.Equal(19.99f, returnedProduct.Product.Prices[0].Price.Value);
    // }

    [Fact]
    public async Task GetAllAsync_WithProductsInDifferentCategories_ShouldReturnAllProducts()
    {
        // Arrange
        var category1 = new Category { Name = "Category 1" };
        var category2 = new Category { Name = "Category 2" };
        var categoryResponse1 = await _fixture.CategoriesClient.CreateCategoryAsync(category1);
        var categoryResponse2 = await _fixture.CategoriesClient.CreateCategoryAsync(category2);

        var product1 = new Product
        {
            Name = "Product in Category 1",
            CategoryKey = categoryResponse1.Key,
            Prices = { new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var product2 = new Product
        {
            Name = "Product in Category 2",
            CategoryKey = categoryResponse2.Key,
            Prices = { new ProductPrice { Value = 19.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        await _fixture.Client.CreateProductAsync(product1);
        await _fixture.Client.CreateProductAsync(product2);

        // Act
        var response = await _fixture.Client.GetAllProductsAsync(new Kaleido.Grpc.Products.EmptyRequest());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.Contains(response.Products, p => p.Product.CategoryKey == categoryResponse1.Key);
        Assert.Contains(response.Products, p => p.Product.CategoryKey == categoryResponse2.Key);
    }
}
