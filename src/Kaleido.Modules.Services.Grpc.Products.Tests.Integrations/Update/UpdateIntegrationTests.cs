using Grpc.Core;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Update;

[Collection("Infrastructure collection")]
public class UpdateIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public UpdateIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var createProduct = new Product
        {
            Name = "Initial Product",
            Description = "Initial Description",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(createProduct);

        var updateRequest = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Updated Product",
                Description = "Updated Description",
                CategoryKey = categoryResponse.Key,
                Prices =
                {
                    new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
                }
            }
        };

        // Act
        var response = await _fixture.Client.UpdateProductAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Updated Product", response.Product.Name);
        Assert.Equal("Updated Description", response.Product.Description);
        Assert.Single(response.Product.Prices.Where(x => x.Revision.Action != "Deleted"));
        Assert.Single(response.Product.Prices.Where(x => x.Revision.Action == "Created"));
        Assert.Equal(19, response.Product.Prices.Where(x => x.Revision.Action == "Created").First().Price.Units);
        Assert.Equal(99, response.Product.Prices.Where(x => x.Revision.Action == "Created").First().Price.Nanos);
    }

    [Fact]
    public async Task UpdateAsync_CategoryIsDeleted_ShouldThrow()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var createProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(createProduct);
        await _fixture.CategoriesClient.DeleteCategoryAsync(new CategoryRequest { Key = categoryResponse.Key });

        var updateRequest = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Updated Product",
                CategoryKey = categoryResponse.Key,
                Prices = { new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.UpdateProductAsync(updateRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_PriceIsRemoved_ShouldMarkAsDeleted()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var currencyKeys = new[]
        {
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        };

        var createProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = currencyKeys[0] },
                new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = currencyKeys[1] }
            }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(createProduct);

        var updateRequest = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Test Product",
                CategoryKey = categoryResponse.Key,
                Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = currencyKeys[0] } }
            }
        };

        // Act
        var response = await _fixture.Client.UpdateProductAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Product.Prices.Count);
        Assert.Single(response.Product.Prices.Where(x => x.Revision.Action == "Unmodified"));
        Assert.Equal(currencyKeys[0], response.Product.Prices.Where(x => x.Revision.Action == "Unmodified").First().Price.CurrencyKey);
        Assert.Equal("Deleted", response.Product.Prices.FirstOrDefault(x => x.Price.CurrencyKey == currencyKeys[1])?.Revision.Action);
    }

    [Fact]
    public async Task UpdateAsync_WhenPriceIsRestored_ShouldBeMarkedAsRestored()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);
        var firstCurrencyKey = Guid.NewGuid().ToString();
        var secondCurrencyKey = Guid.NewGuid().ToString();

        var createProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = firstCurrencyKey },
                new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = secondCurrencyKey }
            }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(createProduct);

        // First update - remove a price
        var updateRequest1 = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Test Product",
                CategoryKey = categoryResponse.Key,
                Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = firstCurrencyKey } }
            }
        };

        await _fixture.Client.UpdateProductAsync(updateRequest1);

        // Second update - restore the price
        var updateRequest2 = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Test Product",
                CategoryKey = categoryResponse.Key,
                Prices =
                {
                    new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = firstCurrencyKey },
                    new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = secondCurrencyKey }
                }
            }
        };

        // Act
        var response = await _fixture.Client.UpdateProductAsync(updateRequest2);

        // Assert
        Assert.NotNull(response);
        Assert.Contains(response.Product.Prices, p => p.Price.CurrencyKey == secondCurrencyKey && p.Revision.Action == "Restored");
        Assert.Equal(2, response.Product.Prices.Count);
    }

    [Fact]
    public async Task UpdateAsync_WithPriceValueUpdate_ShouldUpdatePrice()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var currencyKey = Guid.NewGuid().ToString();
        var createProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = currencyKey } }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(createProduct);

        var updateRequest = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Test Product",
                CategoryKey = categoryResponse.Key,
                Prices = { new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = currencyKey } }
            }
        };

        // Act
        var response = await _fixture.Client.UpdateProductAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Product.Prices);
        Assert.Equal(19, response.Product.Prices[0].Price.Units);
        Assert.Equal(99, response.Product.Prices[0].Price.Nanos);
        Assert.Equal("Updated", response.Product.Prices[0].Revision.Action);
    }

    [Fact]
    public async Task UpdateAsync_WithUnchangedPrice_ShouldMarkAsUnmodified()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var currencyKey = Guid.NewGuid().ToString();
        var createProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = currencyKey } }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(createProduct);

        var updateRequest = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Updated Product", // Change product name but keep same price
                CategoryKey = categoryResponse.Key,
                Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = currencyKey } }
            }
        };

        // Act
        var response = await _fixture.Client.UpdateProductAsync(updateRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Product.Prices);
        Assert.Equal(9, response.Product.Prices[0].Price.Units);
        Assert.Equal(99, response.Product.Prices[0].Price.Nanos);
        Assert.Equal("Unmodified", response.Product.Prices[0].Revision.Action);
    }

    [Fact]
    public async Task UpdateAsync_ProductDoesNotExist_ShouldThrow()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var updateRequest = new ProductActionRequest
        {
            Key = Guid.NewGuid().ToString(),
            Product = new Product
            {
                Name = "Test Product",
                CategoryKey = categoryResponse.Key,
                Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.UpdateProductAsync(updateRequest));
        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    public async Task UpdateAsync_InvalidKey_ShouldThrow(string key)
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var updateRequest = new ProductActionRequest
        {
            Key = key,
            Product = new Product
            {
                Name = "Test Product",
                CategoryKey = categoryResponse.Key,
                Prices = { new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.UpdateProductAsync(updateRequest));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_OnUpdate_ShouldUseSharedTimestamp()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };
        var categoryResponse = await _fixture.CategoriesClient.CreateCategoryAsync(category);

        var createProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = categoryResponse.Key,
            Prices =
            {
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(createProduct);

        var updateRequest = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Updated Product",
                CategoryKey = categoryResponse.Key,
                Prices = { new ProductPrice { Units = 29, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() } }
            }
        };

        // Act
        var response = await _fixture.Client.UpdateProductAsync(updateRequest);

        // Assert
        var updateTimestamps = response.Product.Prices
            .Select(p => p.Revision.CreatedAt)
            .Concat(new[] { response.Revision.CreatedAt })
            .ToList();

        Assert.Single(updateTimestamps.Distinct());
    }
}