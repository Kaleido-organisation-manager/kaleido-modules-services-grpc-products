using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;

public class GetProductRevisionsHandler : IBaseHandler<GetProductRevisionsRequest, GetProductRevisionsResponse>
{
    private readonly IGetProductRevisionsManager _manager;
    private readonly ILogger<GetProductRevisionsHandler> _logger;
    public IRequestValidator<GetProductRevisionsRequest> Validator { get; }

    public GetProductRevisionsHandler(
        IGetProductRevisionsManager manager,
        ILogger<GetProductRevisionsHandler> logger,
        IRequestValidator<GetProductRevisionsRequest> validator)
    {
        _manager = manager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<GetProductRevisionsResponse> HandleAsync(GetProductRevisionsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProductRevisions request for key: {Key}", request.Key);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

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
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}
