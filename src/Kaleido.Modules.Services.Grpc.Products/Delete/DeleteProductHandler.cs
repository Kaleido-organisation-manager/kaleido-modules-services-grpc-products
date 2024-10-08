using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteProductHandler : IBaseHandler<DeleteProductRequest, DeleteProductResponse>
{

    private readonly ILogger<DeleteProductHandler> _logger;
    private readonly IDeleteProductManager _deleteProductManager;

    public DeleteProductHandler(
        ILogger<DeleteProductHandler> logger,
        IDeleteProductManager productsManager
        )
    {
        _logger = logger;
        _deleteProductManager = productsManager;
    }

    public async Task<DeleteProductResponse> HandleAsync(DeleteProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling DeleteProduct request for key: {Key}", request.Key);
        try
        {
            var key = request.Key;
            await _deleteProductManager.DeleteAsync(key, cancellationToken);
            return new DeleteProductResponse();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting product with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}