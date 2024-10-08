using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers;
public class GetProductHandler : IGetProductHandler
{
    private readonly IProductsManager _productsManager;
    private readonly ILogger<GetProductHandler> _logger;

    public GetProductHandler(
        ILogger<GetProductHandler> logger,
        IProductsManager productsManager
        )
    {
        _logger = logger;
        _productsManager = productsManager;
    }

    public async Task<GetProductResponse> HandleAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProduct request for key: {Key}", key);
        Product? product;
        try
        {
            product = await _productsManager.GetProductAsync(key, cancellationToken);
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product with key: {Key} not found", key);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving product with key: {Key}", key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
        return new GetProductResponse
        {
            Product = product
        };
    }
}