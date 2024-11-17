using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Kaleido.Modules.Services.Grpc.Products.Delete;
using Kaleido.Modules.Services.Grpc.Products.Get;
using Kaleido.Modules.Services.Grpc.Products.GetAll;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Services;

public class ProductsService : GrpcProducts.GrpcProductsBase
{
    private readonly ICreateHandler _createHandler;
    private readonly IDeleteHandler _deleteHandler;
    private readonly IGetHandler _getHandler;
    private readonly IGetAllHandler _getAllHandler;

    public ProductsService(
        ICreateHandler createHandler,
        IDeleteHandler deleteHandler,
        IGetHandler getHandler,
        IGetAllHandler getAllHandler)
    {
        _createHandler = createHandler;
        _deleteHandler = deleteHandler;
        _getHandler = getHandler;
        _getAllHandler = getAllHandler;
    }

    public override async Task<ProductResponse> CreateProduct(Product request, ServerCallContext context)
    {
        return await _createHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<ProductResponse> DeleteProduct(ProductRequest request, ServerCallContext context)
    {
        return await _deleteHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<ProductResponse> GetProduct(ProductRequest request, ServerCallContext context)
    {
        return await _getHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<ProductListResponse> GetAllProducts(EmptyRequest request, ServerCallContext context)
    {
        return await _getAllHandler.HandleAsync(request, context.CancellationToken);
    }
}
