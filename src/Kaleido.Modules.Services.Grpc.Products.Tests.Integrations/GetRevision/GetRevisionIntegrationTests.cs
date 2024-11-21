using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetRevision;

[Collection("Infrastructure collection")]
public class GetRevisionIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public GetRevisionIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task GetRevision_WhenProductExists_ReturnsProductRevision()
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
                Description = "Updated Description",
                CategoryKey = categoryResponse.Key,
                Prices =
                {
                    new ProductPrice { Units = 29, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
                }
            }
        };

        await _fixture.Client.UpdateProductAsync(updateRequest);

        // Act
        var request = new ProductRevisionRequest
        {
            Key = createdProduct.Key,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        var response = await _fixture.Client.GetProductRevisionAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Updated Product", response.Product.Name);
        Assert.Equal("Updated Description", response.Product.Description);
        Assert.Single(response.Product.Prices.Where(x => x.Revision.Action != "Deleted"));
        Assert.Equal(2, response.Product.Prices.Where(x => x.Revision.Action == "Deleted").Count());
        Assert.Equal(29, response.Product.Prices.Where(x => x.Revision.Action != "Deleted").First().Price.Units);
        Assert.Equal(99, response.Product.Prices.Where(x => x.Revision.Action != "Deleted").First().Price.Nanos);
    }

    [Fact]
    public async Task GetRevision_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var request = new ProductRevisionRequest
        {
            Key = Guid.NewGuid().ToString(),
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetProductRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    public async Task GetRevision_WithInvalidKey_ThrowsInvalidArgumentException(string key)
    {
        // Arrange
        var request = new ProductRevisionRequest
        {
            Key = key,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetProductRevisionAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetRevision_WhenRevisionDoesNotExist_ThrowsNotFoundException()
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

        var createdProduct = await _fixture.Client.CreateProductAsync(product);

        var request = new ProductRevisionRequest
        {
            Key = createdProduct.Key,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-1)) // Past date with no revisions
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.GetProductRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetRevision_WithDeletedProduct_ReturnsDeletedRevision()
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

        var createdProduct = await _fixture.Client.CreateProductAsync(product);
        await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createdProduct.Key });

        // Act
        var request = new ProductRevisionRequest
        {
            Key = createdProduct.Key,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };
        var response = await _fixture.Client.GetProductRevisionAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Deleted", response.Revision.Action);
    }

    [Fact]
    public async Task GetRevision_WithMultiplePriceChanges_ReturnsCorrectPriceState()
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() },
                new ProductPrice { Units = 19, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        var createdProduct = await _fixture.Client.CreateProductAsync(initialProduct);
        var initialTimestamp = DateTime.UtcNow;

        // Wait a bit to ensure different timestamps
        await Task.Delay(100);

        var updateRequest = new ProductActionRequest
        {
            Key = createdProduct.Key,
            Product = new Product
            {
                Name = "Test Product",
                CategoryKey = categoryResponse.Key,
                Prices =
                {
                    new ProductPrice { Units = 29, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
                }
            }
        };

        await _fixture.Client.UpdateProductAsync(updateRequest);

        // Act
        var request = new ProductRevisionRequest
        {
            Key = createdProduct.Key,
            CreatedAt = Timestamp.FromDateTime(initialTimestamp)
        };
        var response = await _fixture.Client.GetProductRevisionAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Product.Prices.Count);
        Assert.Contains(response.Product.Prices, p => p.Price.Units == 9 && p.Price.Nanos == 99);
        Assert.Contains(response.Product.Prices, p => p.Price.Units == 19 && p.Price.Nanos == 99);
    }
}
