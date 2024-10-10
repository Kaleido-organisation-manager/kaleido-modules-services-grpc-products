using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevision;

public class GetProductRevisionHandler : IBaseHandler<GetProductRevisionRequest, GetProductRevisionResponse>
{
    private readonly IGetProductRevisionManager _getProductRevisionManager;
    private readonly ILogger<GetProductRevisionHandler> _logger;
    public IRequestValidator<GetProductRevisionRequest> Validator { get; }

    public GetProductRevisionHandler(
        IGetProductRevisionManager manager,
        ILogger<GetProductRevisionHandler> logger,
        IRequestValidator<GetProductRevisionRequest> validator
        )
    {
        _getProductRevisionManager = manager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<GetProductRevisionResponse> HandleAsync(GetProductRevisionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProductRevision request for key: {Key} and revision: {Revision}", request.Key, request.Revision);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        ProductRevision? productRevision;
        try
        {
            productRevision = await _getProductRevisionManager.GetAsync(request.Key, request.Revision, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetProductRevision request for key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

        if (productRevision == null)
        {
            _logger.LogWarning("Product revision not found for key: {Key} and revision: {Revision}", request.Key, request.Revision);
            throw new RpcException(new Status(StatusCode.NotFound, "Product revision not found"));
        }

        return new GetProductRevisionResponse
        {
            Revision = productRevision
        };
    }
}