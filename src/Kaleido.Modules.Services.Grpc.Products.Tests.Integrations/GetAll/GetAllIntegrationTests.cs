using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetAll;

public class GetAllIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetAllIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllProducts()
    {
        var client = new GrpcProductsClient(_fixture.Channel);

        List<CreateProduct> productToCreate = [
            new CreateProductBuilder().Build(),
            new CreateProductBuilder().Build(),
            new CreateProductBuilder().Build(),
        ];

        List<Product> createdProducts = [];

        foreach (var product in productToCreate)
        {
            var createResponse = await client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createResponse.Product);
        }

        var response = await client.GetAllProductsAsync(new GetAllProductsRequest());

        Assert.NotNull(response);
        Assert.Equal(createdProducts.Count, response.Products.Count);
        Assert.True(createdProducts.All(product => response.Products.Any(p => p.Key == product.Key)));
    }
}