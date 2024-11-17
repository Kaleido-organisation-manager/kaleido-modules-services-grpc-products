using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Kaleido.Modules.Services.Grpc.Products.Delete;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Services;

public class ProductsService : GrpcProducts.GrpcProductsBase
{
    private readonly ICreateHandler _createHandler;
    private readonly IDeleteHandler _deleteHandler;

    public ProductsService(
        ICreateHandler createHandler,
        IDeleteHandler deleteHandler)
    {
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
    }

    public override async Task<ProductResponse> CreateProduct(Product request, ServerCallContext context)
    {
        return await _createHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<ProductResponse> DeleteProduct(ProductRequest request, ServerCallContext context)
    {
        return await _deleteHandler.HandleAsync(request, context.CancellationToken);
    }
}
