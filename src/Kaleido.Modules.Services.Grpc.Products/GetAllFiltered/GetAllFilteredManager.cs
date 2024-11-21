using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
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

        var nonDeletedProducts = products
            .Where(product => product.Revision.Status == RevisionStatus.Active)
            .Where(product => product.Revision.Action != RevisionAction.Deleted);

        var result = new List<ManagerResponse>();

        foreach (var product in nonDeletedProducts)
        {
            var prices = await _priceLifecycleHandler.FindAllAsync(
                price => price.ProductKey == product.Key,
                cancellationToken: cancellationToken
            );
            var latestPrices = prices
                .Where(price => price.Revision.Status == RevisionStatus.Active)
                .Where(price => price.Revision.Action != RevisionAction.Deleted);
            result.Add(new ManagerResponse(product, latestPrices));
        }

        return result;
    }
}