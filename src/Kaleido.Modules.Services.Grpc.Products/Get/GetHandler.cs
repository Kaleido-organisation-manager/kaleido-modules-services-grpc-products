using Grpc.Core;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public class GetHandler : IBaseHandler<GetProductRequest, GetProductResponse>
{
    private readonly IGetManager _getProductManager;
    private readonly ILogger<GetHandler> _logger;
    public IRequestValidator<GetProductRequest> Validator { get; }

    public GetHandler(
            IGetManager getProductManager,
            ILogger<GetHandler> logger,
            IRequestValidator<GetProductRequest> validator
        )
    {
        _getProductManager = getProductManager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<GetProductResponse> HandleAsync(GetProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetProduct request for key: {Key}", request.Key);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        Product? product;

        try
        {
            product = await _getProductManager.GetAsync(request.Key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving product with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

        if (product == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with key {request.Key} not found"));
        }

        return new GetProductResponse
        {
            Product = product
        };
    }
}