using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public class UpdateProductHandler : IBaseHandler<UpdateProductRequest, UpdateProductResponse>
{
    private readonly ILogger<UpdateProductHandler> _logger;
    private readonly IUpdateProductManager _updateProductManager;

    public UpdateProductHandler(
        ILogger<UpdateProductHandler> logger,
        IUpdateProductManager updateProductManager
        )
    {
        _logger = logger;
        _updateProductManager = updateProductManager;
    }

    public async Task<UpdateProductResponse> HandleAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var productKey = request.Key;
        var product = request.Product;
        _logger.LogInformation("Handling UpdateProduct request for key: {Key}", productKey);
        try
        {
            if (product.Key != productKey)
            {
                var exception = new ValidationException("Product Id does not match the request");
                _logger.LogError(exception, "Product Id does not match the request");
            }
            var updatedProduct = await _updateProductManager.UpdateAsync(product, cancellationToken);
            return new UpdateProductResponse
            {
                Product = updatedProduct
            };
        }
        catch (EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product with key: {Key} not found", productKey);
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while updating product with key: {Key}", productKey);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating product with key: {Key}", productKey);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}