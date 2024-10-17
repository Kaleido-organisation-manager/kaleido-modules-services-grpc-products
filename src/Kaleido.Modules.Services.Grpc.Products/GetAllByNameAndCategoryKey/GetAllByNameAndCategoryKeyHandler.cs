using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByNameAndCategoryKey;

public class GetAllByNameAndCategoryKeyHandler : IBaseHandler<GetAllProductsByNameAndCategoryKeyRequest, GetAllProductsByNameAndCategoryKeyResponse>
{
    private readonly IGetAllByNameAndCategoryKeyManager _getAllByNameAndCategoryKeyManager;
    private readonly ILogger<GetAllByNameAndCategoryKeyHandler> _logger;
    public IRequestValidator<GetAllProductsByNameAndCategoryKeyRequest> Validator { get; }

    public GetAllByNameAndCategoryKeyHandler(
        IGetAllByNameAndCategoryKeyManager getAllByNameAndCategoryKeyManager,
        ILogger<GetAllByNameAndCategoryKeyHandler> logger,
        IRequestValidator<GetAllProductsByNameAndCategoryKeyRequest> validator
        )
    {
        Validator = validator;
        _getAllByNameAndCategoryKeyManager = getAllByNameAndCategoryKeyManager;
        _logger = logger;
    }


    public async Task<GetAllProductsByNameAndCategoryKeyResponse> HandleAsync(GetAllProductsByNameAndCategoryKeyRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllProductsByNameAndCategoryKey request with Name: {Name} and CategoryKey: {CategoryKey}", request.Name, request.CategoryKey);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        try
        {
            var products = await _getAllByNameAndCategoryKeyManager.GetAllByNameAndCategoryKeyAsync(request.Name, request.CategoryKey, cancellationToken);
            return new GetAllProductsByNameAndCategoryKeyResponse
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