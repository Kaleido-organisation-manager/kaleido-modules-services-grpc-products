using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public class GetAllManager : IGetAllManager
{
    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _priceLifecycleHandler;

    public GetAllManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> priceLifecycleHandler)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _priceLifecycleHandler = priceLifecycleHandler;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        var products = await _productLifecycleHandler.GetAllAsync(cancellationToken: cancellationToken);

        var result = new List<ManagerResponse>();

        foreach (var product in products)
        {
            if (product.Revision.Action == RevisionAction.Deleted)
            {
                continue;
            }

            var prices = await _priceLifecycleHandler.FindAllAsync(
                price => price.ProductKey == product.Key,
                cancellationToken: cancellationToken
            );
            var latestPrices = prices.GroupBy(x => x.Key).Select(x => x.OrderByDescending(y => y.Revision.Revision).First())
                .Where(r => r.Revision.Action != RevisionAction.Deleted).ToList();
            result.Add(new ManagerResponse(product, latestPrices));
        }

        return result;
    }
}