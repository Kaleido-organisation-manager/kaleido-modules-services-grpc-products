using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers;
public class GetProductRevisionHandler : IGetProductRevisionHandler
{
    private readonly ILogger<GetProductRevisionHandler> _logger;
    private readonly IProductsManager _productsManager;

    public GetProductRevisionHandler(
        ILogger<GetProductRevisionHandler> logger,
        IProductsManager productsManager
        )
    {
        _logger = logger;
        _productsManager = productsManager;
    }

    public async Task<GetProductRevisionResponse> HandleAsync(string key, int revision, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProductRevision request for key: {Key} and revision: {Revision}", key, revision);
        try
        {
            var productRevision = await _productsManager.GetProductRevisionAsync(key, revision, cancellationToken);

            return new GetProductRevisionResponse
            {
                Revision = productRevision
            };
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product with key: {Key} and revision: {Revision} not found", key, revision);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving product revision with key: {Key} and revision: {Revision}", key, revision);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}