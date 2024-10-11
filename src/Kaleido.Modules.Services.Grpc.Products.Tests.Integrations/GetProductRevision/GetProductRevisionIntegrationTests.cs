using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using Microsoft.VisualBasic;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetProductRevision;

public class GetProductRevisionIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetProductRevisionIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProductRevision_ShouldReturnProduct_WhenProductExists()
    {
        var createProduct = new CreateProductBuilder().Build();
        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var request = new GetProductRevisionRequest { Key = createResponse.Product.Key, Revision = 1 };
        var response = await _fixture.Client.GetProductRevisionAsync(request);

        Assert.NotNull(response.Revision);
        Assert.Equal(createResponse.Product.Key, response.Revision.Key);
        Assert.Equal(1, response.Revision.Revision);
        Assert.Equal("Active", response.Revision.Status);
    }

    [Fact]
    public async Task GetProductRevision_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        var request = new GetProductRevisionRequest { Key = Guid.NewGuid().ToString(), Revision = 1 };

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetProductRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetProductRevision_ShouldReturnNotFound_WhenRevisionDoesNotExist()
    {
        var createProduct = new CreateProductBuilder().Build();
        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var request = new GetProductRevisionRequest { Key = createResponse.Product.Key, Revision = 2 };
        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetProductRevisionAsync(request));
        Assert.Equal(StatusCode.NotFound, exception.Status.StatusCode);
    }

    [Fact]
    public async Task GetProductRevision_ShouldReturnArchived_WhenProductHasBeenArchived()
    {
        var createProduct = new CreateProductBuilder().Build();
        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var updatedProduct = new ProductBuilder()
            .WithKey(createResponse.Product.Key)
            .WithName(createResponse.Product.Name + " - Updated")
            .Build();

        await _fixture.Client.UpdateProductAsync(new UpdateProductRequest { Key = createResponse.Product.Key, Product = updatedProduct });

        var request = new GetProductRevisionRequest { Key = createResponse.Product.Key, Revision = 1 };
        var response = await _fixture.Client.GetProductRevisionAsync(request);

        Assert.NotNull(response.Revision);
        Assert.Equal("Archived", response.Revision.Status);
        Assert.Equal(1, response.Revision.Revision);
    }

    [Fact]
    public async Task GetProductRevision_ShouldReturnDeleted_WhenProductHasBeenDeleted()
    {
        var createProduct = new CreateProductBuilder().Build();
        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var request = new DeleteProductRequest { Key = createResponse.Product.Key };
        await _fixture.Client.DeleteProductAsync(request);

        var getRequest = new GetProductRevisionRequest { Key = createResponse.Product.Key, Revision = 1 };
        var response = await _fixture.Client.GetProductRevisionAsync(getRequest);

        Assert.NotNull(response.Revision);
        Assert.Equal("Deleted", response.Revision.Status);
        Assert.Equal(1, response.Revision.Revision);
    }
}
