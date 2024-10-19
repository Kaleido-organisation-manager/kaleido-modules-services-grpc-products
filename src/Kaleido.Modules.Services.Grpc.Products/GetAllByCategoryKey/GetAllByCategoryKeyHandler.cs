using Grpc.Core;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;

public class GetAllByCategoryKeyHandler : IBaseHandler<GetAllProductsByCategoryKeyRequest, GetAllProductsByCategoryKeyResponse>
{
    private readonly IGetAllByCategoryKeyManager _getAllByCategoryKeyManager;
    private readonly ILogger<GetAllByCategoryKeyHandler> _logger;
    public IRequestValidator<GetAllProductsByCategoryKeyRequest> Validator { get; }

    public GetAllByCategoryKeyHandler(
        IGetAllByCategoryKeyManager getAllByCategoryKeyManager,
        ILogger<GetAllByCategoryKeyHandler> logger,
        IRequestValidator<GetAllProductsByCategoryKeyRequest> validator
        )
    {
        _getAllByCategoryKeyManager = getAllByCategoryKeyManager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<GetAllProductsByCategoryKeyResponse> HandleAsync(GetAllProductsByCategoryKeyRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllProductsByCategoryId request with CategoryId: {CategoryId}", request.CategoryKey);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        try
        {
            var products = await _getAllByCategoryKeyManager.GetAllAsync(request.CategoryKey, cancellationToken);
            return new GetAllProductsByCategoryKeyResponse
            {
                Products = { products }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An internal error occurred");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}