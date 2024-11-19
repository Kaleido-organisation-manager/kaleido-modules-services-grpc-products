using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUpdateManager _manager;
    private readonly IMapper _mapper;
    private readonly KeyValidator _keyValidator;
    private readonly ProductValidator _productValidator;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(
        IUpdateManager manager,
        IMapper mapper,
        KeyValidator keyValidator,
        ProductValidator productValidator,
        ILogger<UpdateHandler> logger)
    {
        _manager = manager;
        _mapper = mapper;
        _keyValidator = keyValidator;
        _productValidator = productValidator;
        _logger = logger;
    }

    public async Task<ProductResponse> HandleAsync(ProductActionRequest request, CancellationToken cancellationToken = default)
    {
        ManagerResponse? result;
        try
        {
            await _keyValidator.ValidateAndThrowAsync(request.Key, cancellationToken);
            await _productValidator.ValidateAndThrowAsync(request.Product, cancellationToken);

            var key = Guid.Parse(request.Key);
            var productEntity = _mapper.Map<ProductEntity>(request.Product);
            var priceEntities = request.Product.Prices
                .Select(p => _mapper.Map<ProductPriceEntity>(p))
                .ToList();

            result = await _manager.UpdateAsync(key, productEntity, priceEntities, cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message, ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }

        if (result == null || result?.State != ManagerResponseState.Success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with key {request.Key} not found"));
        }

        var productWithPricesResult = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(result?.Product);
        productWithPricesResult.Entity.Prices = result?.ProductPrices ?? [];

        return _mapper.Map<ProductResponse>(productWithPricesResult);
    }
}