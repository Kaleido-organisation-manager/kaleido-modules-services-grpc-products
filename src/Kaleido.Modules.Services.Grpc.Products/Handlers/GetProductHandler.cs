using Grpc.Core;
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

    public async Task<GetProductResponse> HandleAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProduct request for Id: {Id}", id);
        var product = await _productsManager.GetProductAsync(id, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product with Id: {Id} not found", id);
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with Id: {id} not found"));
        }
        return new GetProductResponse
        {
            Product = product
        };
    }
}