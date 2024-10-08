using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers;

public class GetProductRevisionsHandler : IGetProductRevisionsHandler
{
    private readonly ILogger<GetProductRevisionsHandler> _logger;
    private readonly IProductsManager _productsManager;

    public GetProductRevisionsHandler(
        ILogger<GetProductRevisionsHandler> logger,
        IProductsManager productsManager
        )
    {
        _logger = logger;
        _productsManager = productsManager;
    }

    public async Task<GetProductRevisionsResponse> HandleAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProductRevisions request for key: {Key}", key);
        try
        {
            var productRevisions = await _productsManager.GetProductRevisionsAsync(key, cancellationToken);

            return new GetProductRevisionsResponse
            {
                Revisions = { productRevisions }
            };
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product with key: {Key} not found", key);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving product revisions with key: {Key}", key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}