using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public class GetAllProductHandler : IBaseHandler<GetAllProductsRequest, GetAllProductsResponse>
{
    private readonly IGetAllProductManager _getAllProductManager;
    private readonly ILogger<GetAllProductHandler> _logger;

    public GetAllProductHandler(
        IGetAllProductManager getAllProductManager,
        ILogger<GetAllProductHandler> logger
        )
    {
        _getAllProductManager = getAllProductManager;
        _logger = logger;
    }

    public async Task<GetAllProductsResponse> HandleAsync(GetAllProductsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Handling GetAllProducts request");
            var products = await _getAllProductManager.GetAllAsync(cancellationToken);
            return new GetAllProductsResponse
            {
                Products = { products }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving products");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}