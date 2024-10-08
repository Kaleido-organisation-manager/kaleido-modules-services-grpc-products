using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public class GetProductHandler : IBaseHandler<GetProductRequest, GetProductResponse>
{
    private readonly IGetProductManager _getProductManager;
    private readonly ILogger<GetProductHandler> _logger;

    public GetProductHandler(
            IGetProductManager getProductManager,
            ILogger<GetProductHandler> logger
        )
    {
        _getProductManager = getProductManager;
        _logger = logger;
    }

    public async Task<GetProductResponse> HandleAsync(GetProductRequest request, CancellationToken cancellationToken = default)
    {
        var key = request.Key;
        _logger.LogInformation("Handling GetProduct request for key: {Key}", key);
        try
        {
            var product = await _getProductManager.GetAsync(key, cancellationToken);
            return new GetProductResponse
            {
                Product = product
            };
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

    }
}