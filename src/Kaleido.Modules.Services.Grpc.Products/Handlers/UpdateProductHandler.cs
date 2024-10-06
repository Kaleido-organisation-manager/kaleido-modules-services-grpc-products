using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers;

public class UpdateProductHandler : IUpdateProductHandler
{
    private readonly IProductsManager _productsManager;

    public UpdateProductHandler(IProductsManager productsManager)
    {
        _productsManager = productsManager;
    }

    public async Task<UpdateProductResponse> HandleAsync(Product product, CancellationToken cancellationToken = default)
    {

        try
        {
            var updatedProduct = await _productsManager.UpdateProductAsync(product, cancellationToken);
            return new UpdateProductResponse
            {
                Product = updatedProduct
            };
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

    }
}