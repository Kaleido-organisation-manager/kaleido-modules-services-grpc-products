using Grpc.Core;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetAllRevisions;

[Collection("Infrastructure collection")]
public class GetAllRevisionsIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetAllRevisionsIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetAllRevisions_WhenProductAndPricesExist_ReturnsProductListResponse()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var createProduct = new Product
        {
            Name = "Test Product",
            Description = "Initial Description",
            CategoryKey = categoryResponse.Key,
            ImageUrl = "https://example.com/image.jpg",
            Prices =
            {
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Value = 19.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(createProduct);

        // Create revision by updating the product
        var updatedProduct = new Product
        {
            Name = "Updated Product",
            Description = "Updated Description",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = 29.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        // Act
        var request = new ProductRequest { Key = createdProduct.Key };
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Products);
        Assert.Equal(2, response.Products.Count); // Expecting 2 revisions

        var latestRevision = response.Products[0];
        var initialRevision = response.Products[1];

        // Check initial revision
        Assert.Equal("Test Product", initialRevision.Product.Name);
        Assert.Equal("Initial Description", initialRevision.Product.Description);
        Assert.Equal(categoryResponse.Key, initialRevision.Product.CategoryKey);
        Assert.Equal(2, initialRevision.Product.Prices.Count);
        Assert.Equal("Created", initialRevision.Revision.Action);

        // Check latest revision
        Assert.Equal("Updated Product", latestRevision.Product.Name);
        Assert.Equal("Updated Description", latestRevision.Product.Description);
        Assert.Equal(categoryResponse.Key, latestRevision.Product.CategoryKey);
        Assert.Single(latestRevision.Product.Prices);
        Assert.Equal("Updated", latestRevision.Revision.Action);
    }

    [Fact]
    public async Task GetAllRevisions_WhenNoRevisionsExist_ReturnsEmptyResponse()
    {
        // Arrange
        var request = new ProductRequest { Key = Guid.NewGuid().ToString() };

        // Act
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Products);
    }

    [Fact]
    public async Task GetAllRevisions_WhenProductIsDeleted_IncludesDeletedRevision()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var product = new Product
        {
            Name = "Test Product to Delete",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(product);
        await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createdProduct.Key });

        // Act
        var request = new ProductRequest { Key = createdProduct.Key };
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.Equal("Deleted", response.Products[0].Revision.Action);
        Assert.Equal("Created", response.Products[1].Revision.Action);
    }

    [Fact]
    public async Task GetAllRevisions_ReflectsPriceChanges()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var initialProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = 9.99f, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Value = 19.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(initialProduct);

        var firstUpdate = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Value = 29.99f, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        await _fixture.Client.UpdateProductAsync(new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = firstUpdate
        });

        var secondUpdate = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Value = 29.99f, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Value = 39.99f, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        await _fixture.Client.UpdateProductAsync(new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = secondUpdate
        });

        // Act
        var request = new ProductRequest { Key = createdProduct.Key };
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        // Assert
        Assert.Equal(3, response.Products.Count);

        var latestRevision = response.Products[0];
        var middleRevision = response.Products[1];
        var initialRevision = response.Products[2];

        // Check initial revision
        Assert.Equal(2, initialRevision.Product.Prices.Count);
        Assert.Equal("Created", initialRevision.Revision.Action);
        Assert.All(initialRevision.Product.Prices, p => Assert.Equal("Created", p.Revision.Action));

        // Check middle revision
        Assert.Single(middleRevision.Product.Prices);
        Assert.Equal("Updated", middleRevision.Revision.Action);
        Assert.Equal(29.99f, middleRevision.Product.Prices[0].Price.Value);

        // Check latest revision
        Assert.Equal(2, latestRevision.Product.Prices.Count);
        Assert.Equal("Updated", latestRevision.Revision.Action);
        Assert.Contains(latestRevision.Product.Prices, p => p.Price.Value == 39.99f);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    public async Task GetAllRevisions_WithInvalidKey_ThrowsInvalidArgument(string key)
    {
        // Arrange
        var request = new ProductRequest { Key = key };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetProductRevisionsAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }
}