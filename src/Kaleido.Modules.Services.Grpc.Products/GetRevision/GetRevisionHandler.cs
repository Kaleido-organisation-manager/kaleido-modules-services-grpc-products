using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Products.GetRevision;

public class GetRevisionHandler : IGetRevisionHandler
{
    private readonly IGetRevisionManager _manager;
    private readonly IMapper _mapper;
    private readonly KeyValidator _keyValidator;
    private readonly ILogger<GetRevisionHandler> _logger;

    public GetRevisionHandler(
        IGetRevisionManager manager,
        IMapper mapper,
        KeyValidator keyValidator,
        ILogger<GetRevisionHandler> logger)
    {
        _manager = manager;
        _mapper = mapper;
        _keyValidator = keyValidator;
        _logger = logger;
    }

    public async Task<ProductResponse> HandleAsync(ProductRevisionRequest request, CancellationToken cancellationToken = default)
    {

        ManagerResponse? result;
        try
        {
            _keyValidator.ValidateAndThrow(request.Key);

            var key = Guid.Parse(request.Key);

            result = await _manager.GetRevisionAsync(key, request.CreatedAt.ToDateTime(), cancellationToken);
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revision");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }

        if (result == null || result.Value.State != ManagerResponseState.Success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product revision not found"));
        }

        var productWithPricesResult = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(result.Value.Product);
        productWithPricesResult.Entity.Prices = result.Value.ProductPrices ?? [];

        return _mapper.Map<ProductResponse>(productWithPricesResult);
    }
}