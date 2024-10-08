using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;

public class GetProductPriceRevisionsHandler : IBaseHandler<GetProductPriceRevisionsRequest, GetProductPriceRevisionsResponse>
{
    private readonly IGetProductPriceRevisionsManager _manager;
    private readonly ILogger<GetProductPriceRevisionsHandler> _logger;
    public GetProductPriceRevisionsHandler(
        IGetProductPriceRevisionsManager manager,
        ILogger<GetProductPriceRevisionsHandler> logger
        )
    {
        _manager = manager;
        _logger = logger;
    }

    public async Task<GetProductPriceRevisionsResponse> HandleAsync(GetProductPriceRevisionsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProductPriceRevisions request for key: {Key}", request.Key);
        try
        {
            var key = request.Key;
            var currency = request.CurrencyKey;
            var revisions = await _manager.GetAllAsync(key, currency, cancellationToken);
            return new GetProductPriceRevisionsResponse
            {
                Revisions = { revisions }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetProductPriceRevisions request for key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
