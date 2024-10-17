using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Get;

public class GetIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Get_ShouldReturnProduct()
    {
        var createProduct = new CreateProductBuilder()
            .Build();

        var createResponse = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = createProduct });

        var request = new GetProductRequest
        {
            Key = createResponse.Product.Key
        };

        var response = await _fixture.Client.GetProductAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Product);
        Assert.Equal(createResponse.Product.Key, response.Product.Key);
    }

    [Fact]
    public async Task Get_ShouldReturnNotFound()
    {
        var request = new GetProductRequest
        {
            Key = Guid.NewGuid().ToString()
        };

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetProductAsync(request));

        Assert.Equal(StatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task Get_IncorrectKeyFormat_ShouldReturnBadRequest()
    {
        var request = new GetProductRequest
        {
            Key = "invalid-key"
        };

        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetProductAsync(request));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }
}