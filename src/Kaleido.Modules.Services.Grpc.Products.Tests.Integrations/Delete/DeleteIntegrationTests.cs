using Grpc.Core;
using Kaleido.Grpc.Categories;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Delete;

[Collection("Infrastructure collection")]
public class DeleteIntegrationTests
{
    private readonly InfrastructureFixture _fixture;

    public DeleteIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearDatabase().Wait();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct()
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
                new ProductPrice { Units = 9, Nanos = 99, CurrencyKey = Guid.NewGuid().ToString() }
            }
        };

        var createResponse = await _fixture.Client.CreateProductAsync(product);

        // Act
        var deleteResponse = await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createResponse.Key });

        // Assert
        Assert.NotNull(deleteResponse);
        Assert.Equal(createResponse.Key, deleteResponse.Key);
        Assert.Equal("Deleted", deleteResponse.Revision.Action);
        Assert.Equal(2, deleteResponse.Revision.Revision);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentKey_ShouldThrowNotFound()
    {
        // Arrange
        var request = new ProductRequest { Key = Guid.NewGuid().ToString() };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.DeleteProductAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    public async Task DeleteAsync_WithInvalidKey_ShouldThrowInvalidArgument(string key)
    {
        // Arrange
        var request = new ProductRequest { Key = key };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.DeleteProductAsync(request));
        Assert.Equal(StatusCode.InvalidArgument, exception.Status.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAssociatedPrices()
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

        var createResponse = await _fixture.Client.CreateProductAsync(product);

        // Act
        var deleteResponse = await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createResponse.Key });

        // Assert
        Assert.NotNull(deleteResponse);
        Assert.Equal(createResponse.Key, deleteResponse.Key);
        Assert.Equal("Deleted", deleteResponse.Revision.Action);
        Assert.Equal(2, deleteResponse.Revision.Revision);

        // Verify all prices are marked as deleted
        foreach (var price in deleteResponse.Product.Prices)
        {
            Assert.Equal("Deleted", price.Revision.Action);
            Assert.Equal(2, price.Revision.Revision);
        }
    }

    [Fact]
    public async Task DeleteAsync_AlreadyDeletedProduct_ShouldThrowNotFound()
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

        var createResponse = await _fixture.Client.CreateProductAsync(product);
        await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createResponse.Key });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _fixture.Client.DeleteProductAsync(new ProductRequest { Key = createResponse.Key }));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }
}
