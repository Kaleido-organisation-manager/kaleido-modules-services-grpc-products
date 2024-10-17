using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetProductPriceRevision;

public class GetProductPriceRevisionIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetProductPriceRevisionIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProductPriceRevision_ShouldReturnProductPriceRevision()
    {
        var productPrice = new ProductPriceBuilder().Build();
        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var request = new GetProductPriceRevisionRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = productPrice.CurrencyKey,
            Revision = 1
        };

        var response = await _fixture.Client.GetProductPriceRevisionAsync(request);

        Assert.NotNull(response.Revision);
        Assert.Equal(productPrice.Value, response.Revision.Value);
        Assert.Equal(productPrice.CurrencyKey, response.Revision.CurrencyKey);
    }

    [Fact]
    public async Task GetProductPriceRevision_ShouldReturnNotFound_WhenProductPriceRevisionDoesNotExist()
    {
        var request = new GetProductPriceRevisionRequest
        {
            Key = Guid.NewGuid().ToString(),
            CurrencyKey = Guid.NewGuid().ToString(),
            Revision = 1
        };

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetProductPriceRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetProductPriceRevision_ShouldReturnRevision_ForHistoricalRevisions()
    {
        var productPrice = new ProductPriceBuilder().Build();
        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var updatedProductPrice = new ProductPriceBuilder()
            .WithCurrencyKey(productPrice.CurrencyKey)
            .WithValue(productPrice.Value + 10)
            .Build();

        var updateProduct = new ProductBuilder()
            .WithKey(createResponse.Product.Key)
            .WithProductPrices([updatedProductPrice])
            .Build();

        await _fixture.Client.UpdateProductAsync(new UpdateProductRequest { Key = updateProduct.Key, Product = updateProduct });

        var request = new GetProductPriceRevisionRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = productPrice.CurrencyKey,
            Revision = 1
        };

        var response = await _fixture.Client.GetProductPriceRevisionAsync(request);

        Assert.NotNull(response.Revision);
        Assert.Equal(productPrice.Value, response.Revision.Value);
        Assert.Equal(productPrice.CurrencyKey, response.Revision.CurrencyKey);
        Assert.Equal(1, response.Revision.Revision);
        Assert.Equal("Archived", response.Revision.Status);
    }

    [Fact]
    public async Task GetProductPriceRevision_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var request = new GetProductPriceRevisionRequest
        {
            Key = Guid.NewGuid().ToString(),
            CurrencyKey = Guid.NewGuid().ToString(),
            Revision = 1
        };

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetProductPriceRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetProductPriceRevision_ShouldReturnNotFound_WhenRevisionDoesNotExist()
    {
        var productPrice = new ProductPriceBuilder().Build();
        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var request = new GetProductPriceRevisionRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = productPrice.CurrencyKey,
            Revision = 2
        };

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetProductPriceRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetProductPriceRevision_ShouldReturnArchived_WhenCurrencyKeyHasBeenChanged()
    {
        var currencyKey = Guid.NewGuid().ToString();
        var productPrice = new ProductPriceBuilder()
            .WithCurrencyKey(currencyKey)
            .Build();

        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var updatedCurrencyKey = Guid.NewGuid().ToString();
        var updatedProductPrice = new ProductPriceBuilder()
            .WithCurrencyKey(updatedCurrencyKey)
            .Build();

        var updateProduct = new ProductBuilder()
            .WithKey(createResponse.Product.Key)
            .WithProductPrices([updatedProductPrice])
            .Build();
        await _fixture.Client.UpdateProductAsync(new UpdateProductRequest { Key = updateProduct.Key, Product = updateProduct });

        var request = new GetProductPriceRevisionRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = currencyKey,
            Revision = 1
        };

        var response = await _fixture.Client.GetProductPriceRevisionAsync(request);

        Assert.NotNull(response.Revision);
        Assert.Equal("Archived", response.Revision.Status);
        Assert.Equal(productPrice.Value, response.Revision.Value);
        Assert.Equal(productPrice.CurrencyKey, response.Revision.CurrencyKey);
        Assert.Equal(1, response.Revision.Revision);

        var newCurrencyKeyRequest = new GetProductPriceRevisionRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = updatedCurrencyKey,
            Revision = 1
        };

        var newCurrencyKeyResponse = await _fixture.Client.GetProductPriceRevisionAsync(newCurrencyKeyRequest);

        Assert.NotNull(newCurrencyKeyResponse.Revision);
        Assert.Equal("Active", newCurrencyKeyResponse.Revision.Status);
        Assert.Equal(updatedProductPrice.Value, newCurrencyKeyResponse.Revision.Value);
        Assert.Equal(updatedProductPrice.CurrencyKey, newCurrencyKeyResponse.Revision.CurrencyKey);
        Assert.Equal(1, newCurrencyKeyResponse.Revision.Revision);
    }

    [Fact]
    public async Task GetProductPriceRevision_ShouldReturnRevision_WhenProductIsDeleted()
    {
        var productPrice = new ProductPriceBuilder().Build();
        var createProduct = new CreateProductBuilder()
            .WithProductPrices([productPrice])
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = createResponse.Product.Key });

        var request = new GetProductPriceRevisionRequest
        {
            Key = createResponse.Product.Key,
            CurrencyKey = productPrice.CurrencyKey,
            Revision = 1
        };

        var response = await _fixture.Client.GetProductPriceRevisionAsync(request);

        Assert.NotNull(response.Revision);
        Assert.Equal("Deleted", response.Revision.Status);
        Assert.Equal(productPrice.Value, response.Revision.Value);
        Assert.Equal(productPrice.CurrencyKey, response.Revision.CurrencyKey);
        Assert.Equal(1, response.Revision.Revision);
    }
}
