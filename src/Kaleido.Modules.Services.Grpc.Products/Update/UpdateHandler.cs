using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public class UpdateHandler : IBaseHandler<UpdateProductRequest, UpdateProductResponse>
{
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IUpdateManager _updateProductManager;
    public IRequestValidator<UpdateProductRequest> Validator { get; }

    public UpdateHandler(
        ILogger<UpdateHandler> logger,
        IUpdateManager updateProductManager,
        IRequestValidator<UpdateProductRequest> validator
        )
    {
        _logger = logger;
        _updateProductManager = updateProductManager;
        Validator = validator;
    }

    public async Task<UpdateProductResponse> HandleAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {

        _logger.LogInformation("Handling UpdateProduct request for key: {Key}", request.Key);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        if (request.Product.Key != request.Key)
        {
            validationResult.AddDataConflictError([nameof(Product), nameof(Product.Key)], "Product key in the request body does not match the key in the URL");
        }
        validationResult.ThrowIfInvalid();

        Product? updatedProduct = null;
        try
        {
            updatedProduct = await _updateProductManager.UpdateAsync(request.Product, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating product with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

        if (updatedProduct == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with key: {request.Key} not found"));
        }

        return new UpdateProductResponse
        {
            Product = updatedProduct
        };
    }
}