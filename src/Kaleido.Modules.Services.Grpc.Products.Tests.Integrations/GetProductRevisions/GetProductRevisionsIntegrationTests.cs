using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetProductRevisions;

public class GetProductRevisionsIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetProductRevisionsIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetProductRevisions_ShouldReturnRevisions_WhenProductExists()
    {
        var createProduct = new CreateProductBuilder().Build();
        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var request = new GetProductRevisionsRequest { Key = createResponse.Product.Key };
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.NotEmpty(response.Revisions);
        Assert.Single(response.Revisions);
        Assert.Equal("Active", response.Revisions[0].Status);
        Assert.Equal(1, response.Revisions[0].Revision);
    }

    [Fact]
    public async Task GetProductRevisions_ShouldReturnRevisions_WhenProductIsDeleted()
    {
        var createProduct = new CreateProductBuilder().Build();
        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });


        var deleteResponse = await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = createResponse.Product.Key });

        var request = new GetProductRevisionsRequest { Key = createResponse.Product.Key };
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.NotEmpty(response.Revisions);
        Assert.Single(response.Revisions);
    }

    [Fact]
    public async Task GetProductRevisions_ShouldReturnRevisions_WhenProductIsUpdated()
    {
        var createProduct = new CreateProductBuilder().Build();
        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var updatedProduct = new ProductBuilder()
            .WithKey(createResponse.Product.Key)
            .WithName("Updated Name")
            .Build();

        var updateResponse = await _fixture.Client.UpdateProductAsync(new UpdateProductRequest { Key = createResponse.Product.Key, Product = updatedProduct });

        var request = new GetProductRevisionsRequest { Key = createResponse.Product.Key };
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.NotEmpty(response.Revisions);
        Assert.Equal(2, response.Revisions.Count);
        Assert.Equal("Archived", response.Revisions.FirstOrDefault(r => r.Revision == 1)?.Status);
        Assert.Equal("Active", response.Revisions.FirstOrDefault(r => r.Revision == 2)?.Status);
    }

    [Fact]
    public async Task GetProductRevisions_ShouldReturnRevisionsWithCorrectKey_ForMulitpleProducts()
    {
        var createProduct1 = new CreateProductBuilder().Build();
        var createResponse1 = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct1 });

        var createProduct2 = new CreateProductBuilder().Build();
        var createResponse2 = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct2 });

        var request = new GetProductRevisionsRequest { Key = createResponse1.Product.Key };
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.NotEmpty(response.Revisions);
        Assert.Single(response.Revisions);
        Assert.Equal(createResponse1.Product.Key, response.Revisions[0].Key);
    }

    [Fact]
    public async Task GetProductRevisions_ShouldReturnEmptyList_WhenProductDoesNotExist()
    {
        var request = new GetProductRevisionsRequest { Key = Guid.NewGuid().ToString() };
        var response = await _fixture.Client.GetProductRevisionsAsync(request);

        Assert.NotNull(response.Revisions);
        Assert.Empty(response.Revisions);
    }
}
