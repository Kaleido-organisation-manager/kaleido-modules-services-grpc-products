using System.Text.Json;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetProductPriceRevisions;

public class GetProductPriceRevisionsIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetProductPriceRevisionsIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProductPriceRevisions_ShouldReturnRevisions_WhenProductExists()
    {
        var productPrice = new ProductPriceBuilder().Build();

        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var request = new GetProductPriceRevisionsRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = productPrice.CurrencyKey
        };

        var response = await _fixture.Client.GetProductPriceRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.Single(response.Revisions);
        Assert.Equal(productPrice.Value, response.Revisions[0].Value);
        Assert.Equal(productPrice.CurrencyKey, response.Revisions[0].CurrencyKey);
        Assert.Equal(1, response.Revisions[0].Revision);
    }

    [Fact]
    public async Task GetProductPriceRevisions_ShouldReturnRevisions_WhenProductIsDeleted()
    {
        var productPrice = new ProductPriceBuilder().Build();

        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = createResponse.Product.Key });

        var request = new GetProductPriceRevisionsRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = productPrice.CurrencyKey
        };

        var response = await _fixture.Client.GetProductPriceRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.Single(response.Revisions);
        Assert.Equal("Deleted", response.Revisions[0].Status);
        Assert.Equal(productPrice.Value, response.Revisions[0].Value);
        Assert.Equal(productPrice.CurrencyKey, response.Revisions[0].CurrencyKey);
        Assert.Equal(1, response.Revisions[0].Revision);
    }

    [Fact]
    public async Task GetProductPriceRevisions_ShouldReturnRevisions_WhenProductPriceHasMultipleRevisions()
    {
        var productPrice = new ProductPriceBuilder().Build();

        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var updateProductPrice = new ProductPriceBuilder()
            .WithValue(10)
            .WithCurrencyKey(productPrice.CurrencyKey)
            .Build();

        var updateResponse = await _fixture.Client.UpdateProductAsync(new UpdateProductRequest
        {
            Key = createResponse.Product.Key,
            Product = new ProductBuilder()
                .WithKey(createResponse.Product.Key)
                .WithProductPrices([updateProductPrice])
                .Build()
        });

        var request = new GetProductPriceRevisionsRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = productPrice.CurrencyKey
        };

        var response = await _fixture.Client.GetProductPriceRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.Equal(2, response.Revisions.Count);
        Assert.Equal("Archived", response.Revisions.FirstOrDefault(r => r.Revision == 1)?.Status);
        Assert.Equal("Active", response.Revisions.FirstOrDefault(r => r.Revision == 2)?.Status);
    }

    [Fact]
    public async Task GetProductPriceRevisions_ShouldReturnRevisions_WhenProductHasMultiplePrices()
    {
        var currencyKey1 = Guid.NewGuid().ToString();
        var productPrice1 = new ProductPriceBuilder().WithCurrencyKey(currencyKey1).Build();

        var currencyKey2 = Guid.NewGuid().ToString();
        var productPrice2 = new ProductPriceBuilder().WithCurrencyKey(currencyKey2).Build();

        var createProduct = new CreateProductBuilder().WithProductPrices([productPrice1, productPrice2]).Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var updateProductPrice1 = new ProductPriceBuilder()
            .WithValue(productPrice1.Value + 10)
            .WithCurrencyKey(currencyKey1)
            .Build();

        var updateResponse = await _fixture.Client.UpdateProductAsync(new UpdateProductRequest
        {
            Key = createResponse.Product.Key,
            Product = new ProductBuilder()
                .WithKey(createResponse.Product.Key)
                .WithProductPrices([updateProductPrice1, productPrice2])
                .Build()
        });

        var request = new GetProductPriceRevisionsRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = currencyKey1
        };

        var response = await _fixture.Client.GetProductPriceRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.Equal(2, response.Revisions.Count);
        Assert.Equal("Archived", response.Revisions.FirstOrDefault(r => r.Revision == 1)?.Status);
        Assert.Equal("Active", response.Revisions.FirstOrDefault(r => r.Revision == 2)?.Status);
    }
}
