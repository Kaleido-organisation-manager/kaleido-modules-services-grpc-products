using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;

public class GetProductRevisionsHandler : IBaseHandler<GetProductRevisionsRequest, GetProductRevisionsResponse>
{
    private readonly IGetProductRevisionsManager _manager;
    private readonly ILogger<GetProductRevisionsHandler> _logger;

    public GetProductRevisionsHandler(
        IGetProductRevisionsManager manager,
        ILogger<GetProductRevisionsHandler> logger)
    {
        _manager = manager;
        _logger = logger;
    }

    public async Task<GetProductRevisionsResponse> HandleAsync(GetProductRevisionsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetProductRevisions request for key: {Key}", request.Key);

        try
        {
            var productRevisions = await _manager.GetAllAsync(request.Key, cancellationToken);
            return new GetProductRevisionsResponse
            {
                Revisions = { productRevisions }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting product revisions for key: {Key}", request.Key);
            throw;
        }
    }
}

