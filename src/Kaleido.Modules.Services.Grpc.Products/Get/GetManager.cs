using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.Extensions.Logging;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public class GetManager : IGetManager
{
    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _productPriceLifecycleHandler;

    public GetManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> productPriceLifecycleHandler)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _productPriceLifecycleHandler = productPriceLifecycleHandler;
    }

    public async Task<ManagerResponse> GetAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var product = await _productLifecycleHandler.GetAsync(key, cancellationToken: cancellationToken);

        if (product == null || product.Revision?.Action == RevisionAction.Deleted)
        {
            return new ManagerResponse(ManagerResponseState.NotFound);
        }

        var prices = await _productPriceLifecycleHandler.FindAllAsync(
            price => price.ProductKey == key,
            revision => revision.Status == RevisionStatus.Active && revision.Action != RevisionAction.Deleted,
            cancellationToken: cancellationToken
        );


        return new ManagerResponse(product, prices);
    }
}