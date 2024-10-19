using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public class GetAllHandler : IBaseHandler<GetAllProductsRequest, GetAllProductsResponse>
{
    private readonly IGetAllManager _getAllProductManager;
    private readonly ILogger<GetAllHandler> _logger;
    public IRequestValidator<GetAllProductsRequest> Validator { get; }

    public GetAllHandler(
        IGetAllManager getAllProductManager,
        ILogger<GetAllHandler> logger,
        IRequestValidator<GetAllProductsRequest> validator
        )
    {
        _getAllProductManager = getAllProductManager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<GetAllProductsResponse> HandleAsync(GetAllProductsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllProducts request");

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        try
        {
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