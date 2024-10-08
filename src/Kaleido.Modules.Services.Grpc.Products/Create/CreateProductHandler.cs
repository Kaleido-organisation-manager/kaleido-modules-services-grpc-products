using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public class CreateProductHandler : IBaseHandler<CreateProductRequest, CreateProductResponse>
{
    private readonly ICreateProductManager _createProductManager;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(
        ICreateProductManager createProductManager,
        ILogger<CreateProductHandler> logger

        )
    {
        _createProductManager = createProductManager;
        _logger = logger;
    }

    public async Task<CreateProductResponse> HandleAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateProduct request for name: {Name}", request.Product.Name);
        try
        {
            return await _createProductManager.CreateAsync(request.Product, cancellationToken);
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating product with name: {Name}", request.Product.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}