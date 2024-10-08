using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevision;

public class GetProductRevisionHandler : IBaseHandler<GetProductRevisionRequest, GetProductRevisionResponse>
{
    private readonly IGetProductRevisionManager _getProductRevisionManager;
    private readonly ILogger<GetProductRevisionHandler> _logger;

    public GetProductRevisionHandler(
        IGetProductRevisionManager manager,
        ILogger<GetProductRevisionHandler> logger
        )
    {
        _getProductRevisionManager = manager;
        _logger = logger;
    }
    public async Task<GetProductRevisionResponse> HandleAsync(GetProductRevisionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProductRevision request for key: {Key} and revision: {Revision}", request.Key, request.Revision);
        try
        {
            var key = request.Key;
            var revision = request.Revision;
            var productRevision = await _getProductRevisionManager.GetAsync(key, request.Revision, cancellationToken);
            return new GetProductRevisionResponse
            {
                Revision = productRevision
            };
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogError(ex, "Entity not found for key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetProductRevision request for key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}