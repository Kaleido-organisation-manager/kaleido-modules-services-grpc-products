using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Update;

public class UpdateIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public UpdateIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Update_ShouldUpdateProduct()
    {
        var createProduct = new CreateProductBuilder().Build();
        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var updatedProduct = new ProductBuilder()
            .WithKey(createResponse.Product.Key)
            .WithName("Updated Name")
            .Build();

        var updateResponse = await _fixture.Client.UpdateProductAsync(new UpdateProductRequest { Key = createResponse.Product.Key, Product = updatedProduct });

        Assert.NotNull(updateResponse);
        Assert.Equal(createResponse.Product.Key, updateResponse.Product.Key);
        Assert.Equal("Updated Name", updateResponse.Product.Name);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var productKey = Guid.NewGuid().ToString();
        var updatedProduct = new ProductBuilder()
            .WithKey(productKey)
            .Build();
        var request = new UpdateProductRequest { Key = productKey, Product = updatedProduct };
        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.UpdateProductAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldStartNewRevisionTree_WhenCurrencyKeyIsChanged()
    {
        var currencyKey = Guid.NewGuid().ToString();
        var productPrice = new ProductPriceBuilder().WithCurrencyKey(currencyKey).Build();
        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var newCurrencyKey = Guid.NewGuid().ToString();
        var updatedProductPrice = new ProductPriceBuilder()
            .WithCurrencyKey(newCurrencyKey)
            .Build();

        var updatedProduct = new ProductBuilder()
            .WithKey(createResponse.Product.Key)
            .WithProductPrices([updatedProductPrice])
            .Build();

        var updateResponse = await _fixture.Client.UpdateProductAsync(new UpdateProductRequest { Key = createResponse.Product.Key, Product = updatedProduct });

        Assert.NotNull(updateResponse);

        var currencyKeyRevisions = await _fixture.Client.GetProductPriceRevisionsAsync(
            new GetProductPriceRevisionsRequest { Key = createResponse.Product.Key, CurrencyKey = currencyKey });

        Assert.Single(currencyKeyRevisions.Revisions);
        Assert.Equal("Archived", currencyKeyRevisions.Revisions[0].Status);

        var newCurrencyKeyRevisions = await _fixture.Client.GetProductPriceRevisionsAsync(
            new GetProductPriceRevisionsRequest { Key = createResponse.Product.Key, CurrencyKey = newCurrencyKey });

        Assert.Single(newCurrencyKeyRevisions.Revisions);
        Assert.Equal("Active", newCurrencyKeyRevisions.Revisions[0].Status);
    }

    [Fact]
    public async Task Update_ShouldHookOnToOldRevision_WhenCurrencyKeyIsReverted()
    {
        var currencyKey = Guid.NewGuid().ToString();
        var productPrice = new ProductPriceBuilder().WithCurrencyKey(currencyKey).Build();
        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var newCurrencyKey = Guid.NewGuid().ToString();
        var updatedProductPrice = new ProductPriceBuilder()
            .WithCurrencyKey(newCurrencyKey)
            .Build();

        var updatedProduct = new ProductBuilder()
            .WithKey(createResponse.Product.Key)
            .WithProductPrices([updatedProductPrice])
            .Build();

        var updateResponse = await _fixture.Client.UpdateProductAsync(new UpdateProductRequest { Key = createResponse.Product.Key, Product = updatedProduct });

        Assert.NotNull(updateResponse);

        var revertedProduct = new ProductBuilder()
            .WithKey(createResponse.Product.Key)
            .WithProductPrices([productPrice])
            .Build();

        var revertResponse = await _fixture.Client.UpdateProductAsync(new UpdateProductRequest { Key = createResponse.Product.Key, Product = revertedProduct });

        Assert.NotNull(revertResponse);

        var currencyKeyRevisions = await _fixture.Client.GetProductPriceRevisionsAsync(
            new GetProductPriceRevisionsRequest { Key = createResponse.Product.Key, CurrencyKey = currencyKey });

        Assert.Equal(2, currencyKeyRevisions.Revisions.Count);
        Assert.Equal("Archived", currencyKeyRevisions.Revisions.FirstOrDefault(x => x.Revision == 1)?.Status);
        Assert.Equal("Active", currencyKeyRevisions.Revisions.FirstOrDefault(x => x.Revision == 2)?.Status);

        var newCurrencyKeyRevisions = await _fixture.Client.GetProductPriceRevisionsAsync(
            new GetProductPriceRevisionsRequest { Key = createResponse.Product.Key, CurrencyKey = newCurrencyKey });

        Assert.Single(newCurrencyKeyRevisions.Revisions);
        Assert.Equal("Archived", newCurrencyKeyRevisions.Revisions[0].Status);
    }
}
