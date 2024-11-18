using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllRevisions;

public class GetAllRevisionsHandler : IGetAllRevisionsHandler
{
    private readonly IGetAllRevisionsManager _manager;
    private readonly IMapper _mapper;
    private readonly KeyValidator _keyValidator;
    private readonly ILogger<GetAllRevisionsHandler> _logger;
    public GetAllRevisionsHandler(
        IGetAllRevisionsManager manager,
        IMapper mapper,
        KeyValidator keyValidator,
        ILogger<GetAllRevisionsHandler> logger)
    {
        _manager = manager;
        _mapper = mapper;
        _keyValidator = keyValidator;
        _logger = logger;
    }

    public async Task<ProductListResponse> HandleAsync(ProductRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _keyValidator.ValidateAndThrow(request.Key);

            var key = Guid.Parse(request.Key);
            var results = await _manager.GetAllRevisionsAsync(key, cancellationToken);

            var response = new ProductListResponse();
            foreach (var item in results)
            {
                var productWithPricesResult = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(item.Product);
                productWithPricesResult.Entity.Prices = item.ProductPrices ?? [];
                response.Products.Add(_mapper.Map<ProductResponse>(productWithPricesResult));
            }

            return response;
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all revisions");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }
    }
}