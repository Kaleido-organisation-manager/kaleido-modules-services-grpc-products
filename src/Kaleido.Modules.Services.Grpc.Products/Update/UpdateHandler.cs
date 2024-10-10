using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

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

        try
        {
            var updatedProduct = await _updateProductManager.UpdateAsync(request.Product, cancellationToken);
            return new UpdateProductResponse
            {
                Product = updatedProduct
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating product with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}