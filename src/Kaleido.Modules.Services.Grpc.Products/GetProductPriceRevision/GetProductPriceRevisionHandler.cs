using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevision;

public class GetProductPriceRevisionHandler : IBaseHandler<GetProductPriceRevisionRequest, GetProductPriceRevisionResponse>
{
    private readonly IGetProductPriceRevisionManager _manager;
    private readonly ILogger<GetProductPriceRevisionHandler> _logger;
    public IRequestValidator<GetProductPriceRevisionRequest> Validator { get; }

    public GetProductPriceRevisionHandler(
        IGetProductPriceRevisionManager manager,
        ILogger<GetProductPriceRevisionHandler> logger,
        IRequestValidator<GetProductPriceRevisionRequest> validator
        )
    {
        _manager = manager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<GetProductPriceRevisionResponse> HandleAsync(GetProductPriceRevisionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProductPriceRevision request for key: {Key}, currency: {Currency} and revision: {Revision}", request.Key, request.CurrencyKey, request.Revision);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        ProductPriceRevision? response;
        try
        {
            response = await _manager.GetAsync(request.Key, request.CurrencyKey, request.Revision, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product price revision for key: {Key} and revision: {Revision}", request.Key, request.Revision);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

        if (response == null)
        {
            _logger.LogWarning("Product price revision not found for key: {Key} and revision: {Revision}", request.Key, request.Revision);
            throw new RpcException(new Status(StatusCode.NotFound, "Product price revision not found"));
        }

        return new GetProductPriceRevisionResponse
        {
            Revision = response
        };
    }
}