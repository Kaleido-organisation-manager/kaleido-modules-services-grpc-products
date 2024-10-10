using Kaleido.Grpc.Products;
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

        var client = new GrpcProductsClient(_fixture.Channel);

        var request = new CreateProductRequest
        {
            Product = CreateProduct()
        };

        var response = await client.CreateProductAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Product);
        Assert.NotNull(response.Product.Key);
    }

    [Fact]
    public async Task Create_ShouldPersistProduct()
    {
        var client = new GrpcProductsClient(_fixture.Channel);

        var request = new CreateProductRequest
        {
            Product = CreateProduct()
        };

        var response = await client.CreateProductAsync(request);
        var product = await client.GetProductAsync(new GetProductRequest { Key = response.Product.Key });

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


    private CreateProduct CreateProduct()
    {
        return new CreateProduct
        {
            Name = "Test Product",
            CategoryKey = Guid.NewGuid().ToString(),
            Description = "Test Product Description",
            ImageUrl = "https://test.com/image.jpg",
            Prices = { new List<ProductPrice> {
                    new ProductPrice
                    {
                        CurrencyKey = Guid.NewGuid().ToString(),
                        Value = 100.00f
                    }
                }}
        };
    }
}