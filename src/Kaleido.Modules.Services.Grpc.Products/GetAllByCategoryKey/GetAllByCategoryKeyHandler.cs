using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;

public class GetAllByCategoryKeyHandler : IBaseHandler<GetAllProductsByCategoryIdRequest, GetAllProductsResponse>
{
    private readonly IGetAllByCategoryKeyManager _getAllByCategoryKeyManager;
    private readonly ILogger<GetAllByCategoryKeyHandler> _logger;

    public GetAllByCategoryKeyHandler(
        IGetAllByCategoryKeyManager getAllByCategoryKeyManager,
        ILogger<GetAllByCategoryKeyHandler> logger
        )
    {
        _getAllByCategoryKeyManager = getAllByCategoryKeyManager;
        _logger = logger;
    }

    public async Task<GetAllProductsResponse> HandleAsync(GetAllProductsByCategoryIdRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllProductsByCategoryId request with CategoryId: {CategoryId}", request.CategoryKey);
        try
        {
            var products = await _getAllByCategoryKeyManager.GetAllAsync(request.CategoryKey, cancellationToken);
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