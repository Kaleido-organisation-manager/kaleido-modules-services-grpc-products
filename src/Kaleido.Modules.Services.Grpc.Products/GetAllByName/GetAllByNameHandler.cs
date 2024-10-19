using Grpc.Core;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByName;

public class GetAllByNameHandler : IBaseHandler<GetAllProductsByNameRequest, GetAllProductsByNameResponse>
{
    private readonly IGetAllByNameManager _getAllByNameManager;
    private readonly ILogger<GetAllByNameHandler> _logger;
    public IRequestValidator<GetAllProductsByNameRequest> Validator { get; }

    public GetAllByNameHandler(
        IGetAllByNameManager getAllByNameManager,
        ILogger<GetAllByNameHandler> logger,
        IRequestValidator<GetAllProductsByNameRequest> validator
        )
    {
        _getAllByNameManager = getAllByNameManager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<GetAllProductsByNameResponse> HandleAsync(GetAllProductsByNameRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling GetAllProductsByName request with Name: {Name}", request.Name);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        try
        {
            var products = await _getAllByNameManager.GetAllByNameAsync(request.Name, cancellationToken);
            return new GetAllProductsByNameResponse
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

