using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers;

public class GetAllProductsByCategoryIdHandler : IGetAllProductsByCategoryIdHandler
{
    private readonly IProductsManager _productsManager;
    private readonly ILogger<GetAllProductsByCategoryIdHandler> _logger;

    public GetAllProductsByCategoryIdHandler(
        IProductsManager productsManager,
        ILogger<GetAllProductsByCategoryIdHandler> logger
        )
    {
        _productsManager = productsManager;
        _logger = logger;
    }

    public async Task<GetAllProductsResponse> HandleAsync(string categoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllProductsByCategoryId request with CategoryId: {CategoryId}", categoryId);
        try
        {
            var categoryKey = Guid.Parse(categoryId);
            var products = await _productsManager.GetAllProductsByCategoryIdAsync(categoryKey, cancellationToken);
            return new GetAllProductsResponse
            {
                Products = { products }
            };
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An internal error occurred");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

    }
}