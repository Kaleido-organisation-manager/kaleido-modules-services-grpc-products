using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Create;

public class CreateIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public CreateIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Create_ShouldCreateProduct()
    {
        var request = new CreateProductRequest
        {
            Product = new CreateProductBuilder().Build()
        };

        var response = await _fixture.Client.CreateProductAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Product);
        Assert.NotNull(response.Product.Key);
    }

    [Fact]
    public async Task Create_ShouldPersistProduct()
    {
        var request = new CreateProductRequest
        {
            Product = new CreateProductBuilder().Build()
        };

        var response = await _fixture.Client.CreateProductAsync(request);
        var product = await _fixture.Client.GetProductAsync(new GetProductRequest { Key = response.Product.Key });

        Assert.NotNull(product);
        Assert.NotNull(product.Product);
        Assert.NotNull(product.Product.Key);
        Assert.Equal(response.Product.Key, product.Product.Key);
        Assert.Equal(request.Product.Name, product.Product.Name);
        Assert.Equal(request.Product.CategoryKey, product.Product.CategoryKey);
        Assert.Equal(request.Product.Description, product.Product.Description);
        Assert.Equal(request.Product.ImageUrl, product.Product.ImageUrl);
        Assert.Equal(request.Product.Prices.Count, product.Product.Prices.Count);
        Assert.Equal(request.Product.Prices[0].CurrencyKey, product.Product.Prices[0].CurrencyKey);
        Assert.Equal(request.Product.Prices[0].Value, product.Product.Prices[0].Value);
    }

}