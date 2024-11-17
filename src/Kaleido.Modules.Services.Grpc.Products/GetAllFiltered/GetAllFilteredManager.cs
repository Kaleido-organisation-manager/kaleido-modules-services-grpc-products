using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllFiltered;

public class GetAllFilteredManager : IGetAllFilteredManager
{
    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _priceLifecycleHandler;

    public GetAllFilteredManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> priceLifecycleHandler)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _priceLifecycleHandler = priceLifecycleHandler;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllFilteredAsync(
        string? name,
        string? categoryKey,
        CancellationToken cancellationToken)
    {
        var products = await _productLifecycleHandler.FindAllAsync(
            product =>
                (string.IsNullOrEmpty(name) || product.Name.ToLower().Contains(name.ToLower())) &&
                (string.IsNullOrEmpty(categoryKey) || product.CategoryKey == Guid.Parse(categoryKey)),
            cancellationToken: cancellationToken
        );

        products = products.GroupBy(v => v.Key)
            .Select(v => v.OrderByDescending(x => x.Revision.Revision).First())
            .Where(v => v.Revision.Action != RevisionAction.Deleted)
            .ToList();

        var result = new List<ManagerResponse>();

        foreach (var product in products)
        {
            var prices = await _priceLifecycleHandler.FindAllAsync(
                price => price.ProductKey == product.Key,
                cancellationToken: cancellationToken
            );
            var latestPrices = prices.GroupBy(x => x.Key)
                .Select(x => x.OrderByDescending(y => y.Revision.Revision).First())
                .Where(r => r.Revision.Action != RevisionAction.Deleted)
                .ToList();
            result.Add(new ManagerResponse(product, latestPrices));
        }

        return result;
    }
}