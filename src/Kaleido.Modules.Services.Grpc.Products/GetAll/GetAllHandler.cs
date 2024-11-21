using AutoMapper;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.Extensions.Logging;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public class GetAllHandler : IGetAllHandler
{
    private readonly IGetAllManager _manager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllHandler> _logger;

    public GetAllHandler(
        IGetAllManager manager,
        IMapper mapper,
        ILogger<GetAllHandler> logger)
    {
        _manager = manager;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductListResponse> HandleAsync(EmptyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _manager.GetAllProductsAsync(cancellationToken);

            var response = new ProductListResponse();

            foreach (var item in result)
            {
                var productWithPricesResult = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(item.Product);
                productWithPricesResult.Entity.Prices = item.ProductPrices ?? [];
                response.Products.Add(_mapper.Map<ProductResponse>(productWithPricesResult));
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all products");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message, ex));
        }
    }
}