using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Delete;

public class DeleteIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public DeleteIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Delete_ShouldDeleteProduct()
    {
        var product = new CreateProductBuilder().Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
        var key = createResponse.Product.Key;

        var deleteResponse = await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = key });

        Assert.Equal(key, deleteResponse.Key);

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetProductAsync(new GetProductRequest { Key = key }));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = Guid.NewGuid().ToString() }));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldSoftDeleteProduct()
    {
        var product = new CreateProductBuilder().Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
        var key = createResponse.Product.Key;

        var deleteResponse = await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = key });

        var revisionsResponse = await _fixture.Client.GetProductRevisionsAsync(new GetProductRevisionsRequest { Key = key });

        Assert.Single(revisionsResponse.Revisions);
        Assert.Equal(product.Name, revisionsResponse.Revisions[0].Name);
        Assert.Equal("Deleted", revisionsResponse.Revisions[0].Status);
    }

}