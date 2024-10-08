using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers;

public class UpdateProductHandler : IUpdateProductHandler
{
    private readonly ILogger<UpdateProductHandler> _logger;
    private readonly IProductsManager _productsManager;

    public UpdateProductHandler(
        ILogger<UpdateProductHandler> logger,
        IProductsManager productsManager
        )
    {
        _logger = logger;
        _productsManager = productsManager;
    }

    public async Task<UpdateProductResponse> HandleAsync(string productKey, Product product, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling UpdateProduct request for key: {Key}", productKey);
        try
        {
            if (product.Key != productKey)
            {
                var exception = new ValidationException("Product Id does not match the request");
                _logger.LogError(exception, "Product Id does not match the request");
            }
            var updatedProduct = await _productsManager.UpdateProductAsync(product, cancellationToken);
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