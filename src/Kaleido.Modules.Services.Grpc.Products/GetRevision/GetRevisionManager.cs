using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.GetRevision;

public class GetRevisionManager : IGetRevisionManager
{
    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _priceLifecycleHandler;
    private readonly IMapper _mapper;

    public GetRevisionManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> priceLifecycleHandler,
        IMapper mapper)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _priceLifecycleHandler = priceLifecycleHandler;
        _mapper = mapper;
    }

    public async Task<ManagerResponse> GetRevisionAsync(Guid key, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        var productRevision = await _productLifecycleHandler.GetHistoricAsync(key, createdAt, cancellationToken);

        if (productRevision == null)
        {
            return new ManagerResponse(ManagerResponseState.NotFound);
        }

        var revisionTimestamp = productRevision.Revision.CreatedAt;
        var productPrices = await _priceLifecycleHandler.FindAllAsync(
            price => price.ProductKey == key,
            r => r.CreatedAt == revisionTimestamp,
            cancellationToken: cancellationToken);

        return new ManagerResponse(productRevision, productPrices);
    }
}