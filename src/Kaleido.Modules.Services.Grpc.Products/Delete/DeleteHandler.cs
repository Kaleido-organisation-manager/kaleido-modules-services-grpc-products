using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteHandler : IBaseHandler<DeleteProductRequest, DeleteProductResponse>
{
    private readonly ILogger<DeleteHandler> _logger;
    private readonly IDeleteManager _deleteProductManager;
    public IRequestValidator<DeleteProductRequest> Validator { get; }

    public DeleteHandler(
        ILogger<DeleteHandler> logger,
        IDeleteManager productsManager,
        IRequestValidator<DeleteProductRequest> validator
        )
    {
        _logger = logger;
        _deleteProductManager = productsManager;
        Validator = validator;
    }

    public async Task<DeleteProductResponse> HandleAsync(DeleteProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling DeleteProduct request for key: {Key}", request.Key);

        var validationResult = await Validator.ValidateAsync(request, cancellationToken);
        validationResult.ThrowIfInvalid();

        ProductEntity? deletedEntity;

        try
        {
            deletedEntity = await _deleteProductManager.DeleteAsync(request.Key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting product with key: {Key}", request.Key);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

        if (deletedEntity == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with key: {request.Key} not found"));
        }

        return new DeleteProductResponse()
        {
            Key = request.Key
        };


    }
}