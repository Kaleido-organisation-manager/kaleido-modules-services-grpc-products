using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;

public class GetProductPriceRevisionsHandler : IBaseHandler<GetProductPriceRevisionsRequest, GetProductPriceRevisionsResponse>
{
    private readonly IGetProductPriceRevisionsManager _manager;
    private readonly ILogger<GetProductPriceRevisionsHandler> _logger;
    public IRequestValidator<GetProductPriceRevisionsRequest> Validator { get; }

    public GetProductPriceRevisionsHandler(
        IGetProductPriceRevisionsManager manager,
        ILogger<GetProductPriceRevisionsHandler> logger,
        IRequestValidator<GetProductPriceRevisionsRequest> validator
        )
    {
        _manager = manager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<GetProductPriceRevisionsResponse> HandleAsync(GetProductPriceRevisionsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProductPriceRevisions request for key: {Key}", request.Key);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        try
        {
            var revisions = await _manager.GetAllAsync(request.Key, request.CurrencyKey, cancellationToken);
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
