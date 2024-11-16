using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Create;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Services;

public class ProductsService : GrpcProducts.GrpcProductsBase
{
    private readonly ICreateHandler _createHandler;

    public ProductsService(ICreateHandler createHandler)
    {
        _createHandler = createHandler;
    }

    public override async Task<ProductResponse> CreateProduct(Product request, ServerCallContext context)
    {
        return await _createHandler.HandleAsync(request, context.CancellationToken);
    }
}