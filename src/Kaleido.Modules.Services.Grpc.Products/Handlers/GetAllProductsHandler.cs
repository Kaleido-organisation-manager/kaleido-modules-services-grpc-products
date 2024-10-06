using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers;

public class GetAllProductsHandler : IGetAllProductsHandler
{
    private readonly IProductsManager _productsManager;
    private readonly ILogger<GetAllProductsHandler> _logger;

    public GetAllProductsHandler(
        IProductsManager productsManager,
        ILogger<GetAllProductsHandler> logger
        )
    {
        _productsManager = productsManager;
        _logger = logger;
    }

    public async Task<GetAllProductsResponse> HandleAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllProducts request");
        var products = await _productsManager.GetAllProductsAsync(cancellationToken);
        return new GetAllProductsResponse
        {
            Products = { products }
        };
    }
}