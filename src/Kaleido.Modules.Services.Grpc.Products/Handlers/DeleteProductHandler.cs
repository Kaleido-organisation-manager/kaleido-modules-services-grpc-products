using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers;

public class DeleteProductHandler : IDeleteProductHandler
{
    private readonly IProductsManager _productsManager;

    public DeleteProductHandler(IProductsManager productsManager)
    {
        _productsManager = productsManager;
    }

    public async Task<DeleteProductResponse> HandleAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _productsManager.DeleteProductAsync(key, cancellationToken);
            return new DeleteProductResponse();
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}