using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevision;

public class GetProductPriceRevisionHandler : IBaseHandler<GetProductPriceRevisionRequest, GetProductPriceRevisionResponse>
{
    private readonly IGetProductPriceRevisionManager _manager;
    private readonly ILogger<GetProductPriceRevisionHandler> _logger;

    public GetProductPriceRevisionHandler(
        IGetProductPriceRevisionManager manager,
        ILogger<GetProductPriceRevisionHandler> logger
        )
    {
        _manager = manager;
        _logger = logger;
    }

    public async Task<GetProductPriceRevisionResponse> HandleAsync(GetProductPriceRevisionRequest request, CancellationToken cancellationToken = default)
    {
        var key = request.Key;
        var currency = request.CurrencyKey;
        var revision = request.Revision;
        _logger.LogInformation("Handling GetProductPriceRevision request for key: {Key}, currency: {Currency} and revision: {Revision}", key, currency, revision);
        try
        {
            var response = await _manager.GetAsync(key, currency, revision, cancellationToken);
            return new GetProductPriceRevisionResponse
            {
                Revision = response
            };
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogError(ex, "Entity not found for key: {Key} and revision: {Revision}", key, revision);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product price revision for key: {Key} and revision: {Revision}", key, revision);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}