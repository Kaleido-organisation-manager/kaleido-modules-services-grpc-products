using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public class CreateHandler : IBaseHandler<CreateProductRequest, CreateProductResponse>
{
    private readonly ICreateManager _createProductManager;
    private readonly ILogger<CreateHandler> _logger;

    public IRequestValidator<CreateProductRequest> Validator { get; }

    public CreateHandler(
        ICreateManager createProductManager,
        ILogger<CreateHandler> logger,
        IRequestValidator<CreateProductRequest> validator
        )
    {
        _createProductManager = createProductManager;
        _logger = logger;
        Validator = validator;
    }

    public async Task<CreateProductResponse> HandleAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateProduct request for name: {Name}", request.Product.Name);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        try
        {
            var storedProduct = await _createProductManager.CreateAsync(request.Product, cancellationToken);
            return new CreateProductResponse
            {
                Product = storedProduct
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating product with name: {Name}", request.Product.Name);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}